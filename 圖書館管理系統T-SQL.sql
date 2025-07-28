--完全版
--------------------------------------------------------------------------------
-- 新的
CREATE TYPE OverDueTVP3 AS TABLE(
	ResultCode INT NULL,
	Message NVARCHAR(MAX) NULL,
	[reservationId] [int] NULL,
	[borrowId] [int] NULL,
	[cid] [int] NULL,
	[collectionId] [int] NULL,
	[bookid] [int] NULL,
	[dueDateB] [datetime2](7) NULL,
	[title] [nvarchar](MAX) NULL,
	[cName] [nvarchar] (MAX) NULL,
	[cAccount] [nvarchar] (MAX) NULL
)
GO

CREATE TYPE OverDueOneToThreeTVP AS TABLE (
	Cid INT NULL,
	ClientName NVARCHAR(50),
	Title NVARCHAR(50),
	Days INT,
	DueDays DATETIME2,
	Email NVARCHAR(100)
)
GO

CREATE TYPE DueBookTVP AS TABLE(
	collectionid INT,
	bookid INT
)
GO
--------------------------------------------------------------------------------
-- 借閱模式_書籍資訊
ALTER PROC BookInfomationForBorrow
@bookCode NVARCHAR(500)
AS
BEGIN
		SELECT bok.bookCode, 
			col.title, 
			auth.author, 
			col.translator, 
			typ.[type], 
			col.publisher, 
			col.publishDate, 
			col.collectionImg, 
			bokstu.bookStatus
		FROM Book bok JOIN Collection col ON bok.collectionId = col.collectionId
		JOIN BookStatus bokstu ON bok.bookStatusId = bokstu.bookStatusId
		JOIN [Type] typ ON col.typeId = typ.typeId
		JOIN Author auth ON auth.authorId = col.authorId
		WHERE bok.bookCode = @bookCode
END
GO
-- 借閱模式主要PROC
ALTER PROC BorrowResult
    @cid INT,
    @bookCode NVARCHAR(500)
AS
BEGIN 
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
    BEGIN TRY
        BEGIN TRAN
        -- 確定該借閱者是否存在
        IF NOT EXISTS( SELECT 1 FROM Client WHERE cId = @cid)
        BEGIN
            SELECT 0  ResultCode, N'該借閱者不存在，請重新輸入!' AS Message;
            ROLLBACK
            RETURN
        END
        
        -- 確認書是否存在
        IF NOT EXISTS ( SELECT 1 FROM Book WHERE bookCode = @bookCode)
        BEGIN
            ROLLBACK
            SELECT 0   ResultCode, N'該本書籍不存在，請重新輸入!' AS Message;
            RETURN
        END

        DECLARE @bookid INT,@collectionId INT, @title NVARCHAR(50)
		SELECT @bookid = bok.bookid, @collectionId = bok.collectionId, @title = col.title
        FROM Book bok
		JOIN Collection col ON bok.collectionId = col.collectionId
        WHERE bookCode = @bookCode
		-- 有逾期的書籍
		IF EXISTS ( SELECT 1 FROM Borrow WHERE cid = @cid AND borrowStatusId = 3 AND returnDate IS NULL)
		BEGIN 
			ROLLBACK
			SELECT 0 ResultCode, N'有逾期未還的書籍' Message
			RETURN
		END
		-- 確認是否借閱超過五本未還
		IF EXISTS ( SELECT 1 FROM Borrow bow WHERE bow.cId = @cid AND bow.borrowStatusId = 2 GROUP BY cid, borrowStatusId HAVING COUNT(cid) > = 5)
		BEGIN 
			ROLLBACK
			SELECT 0 ResultCode, N'已借閱超過五本，無法再借閱' Message
			RETURN
		END
        -- 這本書在館內時 1.可借閱
        IF EXISTS ( SELECT 1 FROM Book WHERE bookCode = @bookCode AND bookStatusId = 1)
        BEGIN

			-- 不能重複借閱同一本collectioinid書
			IF EXISTS ( SELECT 1 
			FROM Borrow bow JOIN Book bok ON bow.bookId = bok.bookId JOIN Collection col ON bok.collectionId = col.collectionId 
			WHERE col.collectionId = @collectionId   AND cid = @cid AND bow.borrowStatusId = 2)
			BEGIN 
				ROLLBACK
				SELECT 0 ResultCode, N'不能重複借閱同一本書籍' Message
				RETURN
			END
			-- 直接更新為借出狀態(1)
            UPDATE Book WITH(ROWLOCK, UPDLOCK) SET bookStatusId = 2
            WHERE bookCode = @bookCode AND bookStatusId = 1;
						
			-- 如果沒有更新成功，代表被預約了....
            IF @@ROWCOUNT = 0
            BEGIN
                SELECT 0  ResultCode, N'該本書籍已被其他人借走' Message;
                ROLLBACK;
                RETURN;
            END
			-- 新增借閱紀錄
            INSERT INTO Borrow (cId, bookid, borrowDate, dueDateB, borrowStatusId)
            VALUES(@cid, @bookid, GETDATE(), DATEADD(DAY, 14, GETDATE()), 2)
			

			DECLARE @borrwId INT
			SELECT @borrwId = SCOPE_IDENTITY();

			INSERT INTO History (borrowId)
			VALUES (@borrwId)

            SELECT 1 AS ResultCode, N'借閱成功' AS Message;
            COMMIT
            RETURN
        END
        -- 如果有在網路上預約(2)的話
        DECLARE @reservationId INT;
        IF EXISTS (
            SELECT 0 FROM Book WHERE bookCode = @bookCode AND bookStatusId = 2
        )
        BEGIN
            -- 判斷該人是否是 收到通知的預約者前來領書 => 狀態: 待取書中
            SELECT @reservationId = reservationId
            FROM Reservation WITH(UPDLOCK, ROWLOCK)
            WHERE cid = @cid AND reservationStatusId = 3
            AND bookId = @bookId

            IF @reservationId IS NULL
            BEGIN 
                SELECT 0 AS ResultCode, N'此書籍已被借閱!' AS Message;
                ROLLBACK
				RETURN
            END

            --  改成預約狀態 已借出
            UPDATE Reservation SET reservationStatusId = 1
            WHERE reservationId = @reservationId AND cid = @cid

            IF @@ROWCOUNT = 0
            BEGIN 
                SELECT 1  ResultCode, N'書籍狀態已變更，無法完成借書' AS Message;
                ROLLBACK;
                RETURN;
            END

			-- 插入借閱紀錄
            INSERT INTO Borrow (cId,reservationId, bookId, borrowDate, dueDateB, borrowStatusId)
            VALUES(@cid, @reservationId , @bookId, GETDATE(), DATEADD(DAY, 14, GETDATE()), 2)

            SELECT 1 AS ResultCode, N'借閱成功' AS Message;
            COMMIT;
            RETURN;
        END
        -- 
        SELECT 0 ResultCode, N'該本書籍無法借閱。' AS Message;
        ROLLBACK
        RETURN
    END TRY
    BEGIN CATCH 
        ROLLBACK
        SELECT 500 ResultCode, N'發生錯誤' Message
    END CATCH
END
GO
--------------------------------------------------------------------------------
-- 預約模式_關鍵字書籍搜尋
ALTER PROC BookStatusDetail
    @keyword NVARCHAR(500),
    @bookstatus NVARCHAR(20)
AS 
BEGIN 
    SELECT col.collectionId ,title, author, bookstatus, 
	( SELECT COUNT(*) FROM Reservation r WHERE r.collectionId = col.collectionId AND reservationStatusId = 2) number
    FROM Collection col 
	JOIN Book bok ON col.collectionId = bok.collectionId
    JOIN BookStatus bokstu ON bok.bookStatusId = bokstu.bookStatusId
	JOIN Author auth ON auth.authorId = col.authorId
    LEFT JOIN Reservation re ON col.collectionId = re.collectionId
    WHERE (col.title LIKE '%'+ @keyword + '%' OR auth.author LIKE '%' + @keyword + '%') 
    AND 
            (@bookstatus = 'ALL' AND bok.bookStatusId IN (1,2, 3) OR
            (@bookstatus = 'IsLent' AND bok.bookStatusId IN (2)) OR
            (@bookstatus = 'Available' AND bok.bookStatusId = 1)
    )
    GROUP BY  col.collectionId, title, author, bokstu.bookStatus
END;
GO

-- 預約模式_預約通知
ALTER PROC NotificationAppointmentSuccess
    @cid INT,
    @collectionId INT
AS
BEGIN
	SET NOCOUNT ON
    BEGIN TRY
        DECLARE @message NVARCHAR(500); 
		DECLARE @title NVARCHAR(500); 
		DECLARE @cName NVARCHAR(20);

        SELECT @cName = cName FROM Client WHERE cid = @cid
        SELECT @title = title FROM Collection WHERE collectionId = @collectionId
		IF (@cName IS NULL OR @title IS NULL)
		BEGIN 
			SELECT 0 ResultCode, '通知失敗: 找不到預約者或書籍' Message
			RETURN
		END

        SET @message = N'【預約成功通知】親愛的 ' + @cName +
                       N' ，您已於'  + CONVERT(NVARCHAR ,FORMAT(GETDATE(), 'yyyy-MM-dd'))+ N' 預約了 '+ @title + 
                       N' 請耐心等候通知。圖書館管理系統 敬上。';

        INSERT INTO Notification (cid, [message], notificationDate) VALUES (@cid,  @message, GETDATE());
		
    END TRY
    BEGIN CATCH
		SELECT 0 ResultCode, '通知失敗: ' + ERROR_MESSAGE() Message
        RETURN
    END CATCH
END
GO

--預約模式主要PROC
ALTER PROC AppointmentMode
    @cid INT,
    @collectionId INT
AS	
BEGIN 
	SET NOCOUNT ON
    BEGIN TRY
        BEGIN TRANSACTION
        -- 先確認使用者是否存在
        IF NOT EXISTS ( SELECT 1 FROM Client WHERE cid = @cid)
        BEGIN
			ROLLBACK
            SELECT 0 ResultCode, '使用者不存在' Message 
			RETURN
        END
		DECLARE @title NVARCHAR(50)
		SELECT @title = title
		FROM Collection WHERE collectionId = @collectionId
		-- 逾期未還
        IF EXISTS (SELECT 1 FROM Borrow WHERE cid = @cid AND borrowStatusId = 3 AND returnDate IS NULL)
		BEGIN 
			ROLLBACK
			SELECT 0 ResultCode, N'有逾期未還的書籍' Message
			RETURN 
		END
        -- 確認書籍是否存在
        IF NOT EXISTS (SELECT 1 FROM Collection WHERE collectionId = @collectionId)
        BEGIN
			ROLLBACK
            SELECT 0 ResultCode, '書籍不存在' Message  
			RETURN
        END 
        -- 如果預約已經存在
        IF EXISTS (SELECT 1 FROM Reservation WHERE cid=@cid AND collectionId = @collectionId AND reservationStatusId  = 2 )
        BEGIN 
				ROLLBACK
				SELECT 0 ResultCode, '重複預約' Message
				RETURN
		END
        -- 如果該本書 在館內 
        IF EXISTS (SELECT 1 FROM Book WHERE collectionId = @collectionId AND  bookStatusId = 1)
        BEGIN 
                ROLLBACK 
                SELECT 0 ResultCode, '此本書目前在本館' Message
				RETURN
        END 
		-- 如果你已經借閱該本書了，就不能再預約
		IF EXISTS ( SELECT 1 FROM Borrow bor JOIN Book bok ON bor.bookId = bok.bookId WHERE bok.collectionId = @collectionId AND bor.cId = @cid AND bor.borrowStatusId = 2)
		BEGIN
			ROLLBACK 
            SELECT 0 ResultCode, '已借閱相同的書籍，無法再進行預約' Message
			RETURN
		END
        -- 如果該本書有預約 
        IF EXISTS (SELECT 1 FROM Book WHERE bookStatusId  = 2 AND collectionId = @collectionId)
        BEGIN 
            INSERT INTO Reservation (cId, collectionId, reservationDate,reservationStatusId)
			VALUES (@cid, @collectionid, GETDATE(), 2)
			EXEC NotificationAppointmentSuccess @cid,@collectionid
            SELECT 1 ResultCode, '預約成功' Message 
            COMMIT 
            RETURN 
        END 

        ROLLBACK
		SELECT 0 ResultCode, '預約失敗' Message
		RETURN
    END TRY

    BEGIN CATCH 
		ROLLBACK
		SELECT 0 ResultCode, '出現錯誤: ' + ERROR_MESSAGE() Message 
		RETURN
	END CATCH
END
GO
--------------------------------------------------------------------------------
--還書模式_取得最早預約者TVF  
ALTER FUNCTION GetEarliestReservation(@collectionId INT)
RETURNS TABLE
AS 
RETURN
(
	WITH ReservationCTE AS (
			SELECT reservationId,  collectionId, cid, reservationDate, rs.reservationStatus,ROW_NUMBER() OVER(PARTITION BY collectionId  ORDER BY  reservationDate ASC) rk
			FROM Reservation re  JOIN ReservationStatus rs ON re.reservationStatusId =  rs.reservationStatusId
			WHERE collectionId = @collectionId AND  re.reservationStatusId = 2
	)
    SELECT reservationId, collectionId, cid,reservationDate, reservationStatus
    FROM ReservationCTE
    WHERE rk = 1
)
GO

--還書模式_通知預約者取書
ALTER PROC NotificationBooker
    @cid INT,
    @collectionId INT
AS
BEGIN
	SET NOCOUNT ON
    BEGIN TRY
        DECLARE @message NVARCHAR(500); 
		DECLARE @title NVARCHAR(500); 
		DECLARE @cName NVARCHAR(20);

        SELECT @cName = cName FROM Client WHERE cid = @cid
        SELECT @title = title FROM Collection WHERE collectionId = @collectionId
		IF (@cName IS NULL OR @title IS NULL)
		BEGIN 
			RETURN
		END

        SET @message = N'【取書通知】親愛的 ' + @cName +
                       N' ，您所預約的書 ' + @title + 
                       N' 已可以借閱，請於' + CONVERT(NVARCHAR, FORMAT(DATEADD(DAY, 2, GETDATE()), 'yyyy-MM-dd'), 111)+'內到本館借書，謝謝。圖書館管理系統 敬上。';
        INSERT INTO Notification (cid, [message], notificationDate) VALUES (@cid,  @message, GETDATE());
    END TRY
    BEGIN CATCH
		SELECT 0 ResultCode, N'通知失敗: ' + ERROR_MESSAGE() Message
        RETURN
    END CATCH
END
GO

--還書模式_檢查是否有其他預約
ALTER PROC CheckBookIsReservation
    @collectionId int,
	@bookid INT
AS 
BEGIN
	SET NOCOUNT ON
    BEGIN TRY
        DECLARE @cid INT;
        DECLARE @reservationId INT;

        --取的該本書最早預約的
        SELECT @reservationId = reservationId, @cid = cid
        FROM GetEarliestReservation(@collectionId)
		IF (@reservationId IS NULL OR @cid IS NULL)
		BEGIN 
				SELECT 0 ResultCode, N'無有效預約紀錄' Message, NULL AS cName, NULL AS cAccount, NULL AS title
				RETURN
		END
        -- 更新預約狀態"可取書"、三天取書時間
        UPDATE Reservation WITH(ROWLOCK, UPDLOCK)
        SET bookid = @bookid, reservationStatusId = 3, dueDateR = DATEADD(DAY, 3, GETDATE())
        WHERE reservationId = @reservationId AND cid = @cid
		AND reservationStatusId = 2

        IF @@ROWCOUNT = 0
        BEGIN 
            SELECT 0 ResultCode, N'更改預約者狀態失敗' MESSAGE, NULL AS cName, NULL AS cAccount, NULL AS title
            RETURN
        END
		-- 通知預約者取書
        EXEC NotificationBooker @cid, @collectionId

		-- 回傳
        SELECT 1 ResultCode, N'回傳預約者取書。' Message, cli.cName, cli.cAccount AS cAccount , col.title
		FROM Client cli JOIN Reservation re ON cli.cId = re.cId
		JOIN Collection col ON re.collectionId = col.collectionId
		WHERE cli.cId = @cid AND re.collectionId = @collectionId

    END TRY
    BEGIN CATCH
        SELECT 0 ResultCode, N'發生錯誤:' + ERROR_MESSAGE() Message, NULL AS cName, NULL AS cAccount, NULL AS title
        RETURN
    END CATCH
END 
GO

--還書模式_主要PROC
ALTER PROC ReturnBook
			@BookCode NVARCHAR(500)
AS
BEGIN
		SET TRANSACTION ISOLATION LEVEL READ COMMITTED
		SET NOCOUNT ON
		BEGIN TRY
				BEGIN TRAN
				DECLARE @borrwid INT
				DECLARE @collectionId INT
				DECLARE @bookid INT

				-- 找出借閱紀錄
				SELECT TOP 1 @borrwid = borrowid, @collectionId = collectionId, @bookid = bok.bookid
				FROM Borrow bor JOIN Book bok ON bor.bookId  = bok.bookId
				WHERE bookCode = @BookCode AND bor.borrowStatusId IN ( 2,3) 
				
				-- 確認是否有被借閱
				IF @borrwid IS NULL
				BEGIN 
					ROLLBACK
					SELECT 0 ResultCode, @BookCode + N'並未被借閱。' Message, NULL collectionid, NULL bookid
					RETURN
				END

				-- 確認是該本書被借閱 => 要歸還
				IF  @borrwid IS NOT NULL 
				BEGIN
					-- 如果沒已借閱者
					-- 更新 還書者狀態 已歸還(1)
					UPDATE Borrow WITH(ROWLOCK, UPDLOCK) SET borrowStatusId = 1, returnDate = GETDATE()
					WHERE borrowId = @borrwid 
					
					IF @@ROWCOUNT = 0 
					BEGIN 
						SELECT 0 ResultCode, N'歸還失敗' Message, NULL collectionid, NULL bookid
						ROLLBACK
						RETURN
					END

					--  如果有其他借閱者
					IF EXISTS ( SELECT 1 FROM Reservation re WHERE collectionId = @collectionId AND reservationStatusId = 2 )
					BEGIN
						COMMIT
						SELECT 1 ResultCode,   N'有其他借閱者回傳'  Message, @collectionId AS collectionid, @bookid  AS bookid
						RETURN 
					END

					-- 如果沒有其他借閱者~更新Book可借閱(1)
					UPDATE Book WITH(ROWLOCK, UPDLOCK) SET bookStatusId = 1
					WHERE bookCode = @BookCode  

					IF @@ROWCOUNT = 0 
					BEGIN 
						SELECT 0 ResultCode, N'更新書籍可借閱狀態失敗' Message, NULL collectionid, NULL bookid
						ROLLBACK
						RETURN
					END

					SELECT 1 ResultCode, (@BookCode + N'歸還成功!' ) Message, NULL collectionid, NULL bookid
					COMMIT
					RETURN
				END

				-- 該本書不存在
				ROLLBACK
				SELECT 0 ResultCode, N'該本書不存在' Message
				RETURN
		END TRY
		BEGIN CATCH
				SELECT 0 ResultCode, N'發生錯誤: '+ ERROR_MESSAGE() Message
				ROLLBACK
				RETURN
		END CATCH
END
GO
--------------------------------------------------------------------------------
---即將逾期提醒_通知
ALTER PROC NotificationAboutToExpireToEmail
	@OverDue OverDueOneToThreeTVP READONLY
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO Notification (cid, message, notificationDate)
	SELECT
		Cid,
		N'【即將逾期通知】親愛的 ' + ClientName +
		N' ，您所借閱的 ' + Title +
		N' 將於 ' + CONVERT(NVARCHAR, FORMAT(DueDays, 'yyyy-MM-dd'), 111) +
		N' 逾期，距離還書期限僅剩' + CONVERT(NVARCHAR, Days) +
		N' 天。請儘速歸還。圖書館管理系統 敬上。',
		GETDATE()
	FROM @OverDue od
	SELECT 1 ResultCode, N'所有即將預期通知已發放成功' Message 
END
GO
--------------------------------------------------------------------------------
-- 1 取書逾期排程_主要PROC
-- 功能: 找出逾期者們，更新其狀態、執行1.2通知、回傳逾期者們及書籍，進行郵件發送。
ALTER PROC OverDue
AS
BEGIN
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED
	BEGIN TRY
		BEGIN TRANSACTION
		DECLARE @OverDueList OverDueTVP3
		-- 儲存取書逾期者
		INSERT INTO @OverDueList ( reservationId, cid, collectionId, bookId, dueDateB )
		SELECT reservationId, cid, collectionId, bookId, dueDateR
		FROM Reservation
		WHERE reservationStatusId = 3 AND dueDateR < GETDATE()

		-- 檢查是否有取書逾期者
		IF NOT EXISTS ( SELECT 1 FROM @OverDueList)
		BEGIN
			ROLLBACK
			SELECT 0 ResultCode, N'沒有逾期者!' Message, NULL AS reservationId  , NULL AS cid, NULL AS  borrowId, NULL AS  collectionId, NULL AS bookId, NULL AS  cName, NULL AS  title, NULL AS  cAccount, NULL AS  dueDateB
			RETURN
		END
		
		-- 取書逾期者更改狀態 取書逾期
		UPDATE Reservation WITH(ROWLOCK,UPDLOCK) SET reservationStatusId = 4
		WHERE reservationId IN ( SELECT reservationId FROM @OverDueList);
		
		IF (@@ROWCOUNT = 0)
		BEGIN 
			ROLLBACK
			SELECT 0 ResultCode, N'無逾期紀錄。' Message
			RETURN
		END

		-- 站內通知逾期者們 
		EXEC NotificationOverdue @OverDue = @OverDueList

		-- 回傳 @OverDueList
		SELECT 1 ResultCode, N'逾期者們' Message,  reservationId, ov.cid,ov.borrowId, ov.collectionId, bookId, cli.cName, col.title, cli.cAccount, ov.dueDateB
		FROM @OverDueList ov
		JOIN Client cli ON ov.cid = cli.cId
		JOIN Collection col ON ov.collectionId = col.collectionId

		COMMIT
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK;
		SELECT 0 ResultCode, N'執行失敗' + ERROR_MESSAGE() Message;
	END CATCH
END
GO
-- 1.2 通知預期者已取消取書
ALTER PROC NotificationOverdue
    @OverDue OverDueTVP3 READONLY
AS
BEGIN
	SET NOCOUNT ON
    BEGIN TRY
		INSERT INTO Notification (cid, message, notificationDate )
		SELECT
				od.cid,
				N'【取書逾期通知】親愛的 ' + cli.cName +
                       N' ，您所預約的 【' + col.title + 
                       N'】 未於 ' + CONVERT(NVARCHAR, FORMAT(DATEADD(DAY, -1, GETDATE()), 'yyyy-MM-dd'), 111) +' 前取書，系統已取消，如有需要請重新預約，謝謝。圖書館管理系統 敬上。',
				GETDATE()
		FROM @OverDue od
		JOIN Client cli ON od.cid = cli.cid 
		JOIN Collection col ON col.collectionId = od.collectionId
    END TRY
    BEGIN CATCH
        RETURN
    END CATCH
END
GO

-- 2.1 檢查其他預約者並更新書籍狀態
-- 功能: 接續1的回傳結果，使用2.2 TVF找出下一位預約者們
-- ，執行2.3通知、回傳預約者們，進行郵件發送。
ALTER PROC CheckReservationSchedule
		@DueBook DueBookTVP READONLY
AS 
BEGIN
	SET NOCOUNT ON
    BEGIN TRY
		-- 1用來存放-書籍沒有預約者的TVP
		DECLARE @bookList  OverDueTVP3;
		-- 2用來存放-書籍有其他預約者的TVP
		DECLARE @reservationList OverDueTVP3;
		
		-- 1.取得沒有預約的書
		INSERT INTO @bookList (collectionId, bookid)
		SELECT  collectionId, bookid 
		FROM @DueBook bok 
		WHERE NOT EXISTS 
		( SELECT 1 FROM Reservation re WHERE re.collectionId = bok.collectionId   AND reservationStatusId IN ( 2, 3)  )

		 --1.1 更新書籍狀態為 1.可借閱
		IF EXISTS (SELECT 1 FROM @bookList)
		BEGIN
					UPDATE  Book WITH(ROWLOCK, UPDLOCK) SET bookStatusId = 1
					FROM Book bok JOIN @bookList boklist ON bok.bookid = boklist.bookid AND bok.collectionId = boklist.collectionId
		END

        --2.取得書籍最早預約的人 ok
		INSERT INTO @reservationList (reservationId, cid, bookid,collectionid )
        SELECT reservationId,  cid,bookid,collectionid 
        FROM GetEarliestReservationTVF(@DueBook)
		
        -- 2.2更新預約狀態"可取書"、三天取書時間 ok
        UPDATE Reservation WITH(ROWLOCK, UPDLOCK)
        SET bookid = rlist.bookid, reservationStatusId = 3, dueDateR = DATEADD(DAY, 3, GETDATE())
        FROM Reservation re JOIN @reservationList rlist ON re.reservationId = rlist.reservationId

		--2.3通知預約者 ok
		IF EXISTS ( SELECT 1 FROM @reservationList)
		BEGIN
				EXEC NotificationBookerTVPvsersion  @reservationer = @reservationList
		END

		--這裡要回傳到 出去! 要有名字 、書籍
        SELECT 1 ResultCode, N'回傳下一位預約者名單' Message, u.cName, col.title, u.cAccount
		FROM @reservationList list
		JOIN Collection col ON list.collectionId = col.collectionId
		JOIN Client u ON u.cId = list.cid

    END TRY
    BEGIN CATCH
        SELECT 0 ResultCode, N'發生錯誤:' + ERROR_MESSAGE() Message,  NULL Name, NULL title, NULL cAccount
        RETURN
    END CATCH
END 
GO
-- 2.2 取得書籍最早預約TVF
ALTER FUNCTION GetEarliestReservationTVF
(
	@InputTable DueBookTVP READONLY
)
RETURNS @RESULT TABLE (
		reservationId INT,
		collectionid INT,
		cid INT,
		bookid INT,
		reservationDate DATETIME,
		reservationStatus NVARCHAR(50)
)
AS 
BEGIN
		INSERT INTO @RESULT
		SELECT 
				re.reservationId,
				re.collectionid,
				re.cId,
				re.bookid,
				re.reservationDate,
				rs.reservationStatus
		FROM (
			SELECT 
				re.reservationId,  
				re.collectionid, 
				re.cid, 
				tvp.bookId,
				re.reservationDate, 
				re.reservationStatusId,
				ROW_NUMBER() OVER(PARTITION BY re.collectionId  ORDER BY  re.reservationDate ASC) rk
				FROM Reservation re
				JOIN @InputTable tvp ON re.collectionid = tvp.collectionid
				WHERE re.reservationStatusId = 2
			) AS re
			JOIN ReservationStatus rs ON re.reservationStatusId = rs.reservationStatusId
			WHERE re.rk = 1
			RETURN;
END
GO
-- 2.3 通知預約者們取書
ALTER PROC NotificationBookerTVPvsersion
    @reservationer OverDueTVP3 READONLY
AS
BEGIN
	SET NOCOUNT ON
    BEGIN TRY
			INSERT INTO Notification (cid, [message], notificationDate)
			SELECT  
				cli.cid,
				N'【取書通知】親愛的 ' + cli.cName +
                       N' ，您所預約的書 【' + col.title + 
                       N'】已可以借閱，請於'+ CONVERT(NVARCHAR, FORMAT(DATEADD(DAY, 2, GETDATE()), 'yyyy-MM-dd'), 111)+'天內到本館借書。圖書館管理系統 敬上。',
					   GETDATE()
			FROM @reservationer re 
			JOIN Client cli ON re.cid = cli.cid
			JOIN Collection col ON re.collectionId = col.collectionId
    END TRY
    BEGIN CATCH
        RETURN
    END CATCH
END
GO

--------------------------------------------------------------------------------
--借閱逾期排程_預期通知
ALTER PROC NotificationOverdueBorrow
    @OverDue OverDueTVP2 READONLY
AS
BEGIN
	SET NOCOUNT ON
    BEGIN TRY
		INSERT INTO Notification (cid, message, notificationDate )
		SELECT
				od.cid,
				N'【逾期警告通知】親愛的 ' + cli.cName +
                       N' ，您所借閱的 ' + col.title + 
                       N' 已經逾期，請盡速還書。 圖書館管理系統 敬上。',
				GETDATE()
		FROM @OverDue od
		JOIN Client cli ON od.cid = cli.cid 
		JOIN Book bok ON od.bookid = bok.bookId
		JOIN Collection col ON bok.collectionId = col.collectionId
    END TRY
    BEGIN CATCH
        RETURN
    END CATCH
END
GO

--借閱逾期排程_ 主要借閱預期PROC 
CREATE PROC LateReturn2Email
AS
BEGIN
		SET NOCOUNT ON
		SET TRANSACTION ISOLATION LEVEL READ COMMITTED
		BEGIN TRY
			BEGIN TRANSACTION
			DECLARE @DueList OverDueTVP2;
			

			INSERT INTO @DueList ( cid, bookid,borrowId, dueDateB)
			SELECT  cid, bookid,borrowId, dueDateB
			FROM Borrow
			WHERE dueDateB < GETDATE() AND borrowStatusId = 2 OR (  borrowStatusId = 3 AND  returnDate IS NULL)

			IF NOT EXISTS ( SELECT 1 FROM @DueList)
			BEGIN
				SELECT 0 ResultCode, N'無逾期歸還者' Message, NULL cName , NULL cAccount , NULL title  
				ROLLBACK
				RETURN 
			END

			 --更新逾期歸還者的狀態(逾期)
			UPDATE Borrow WITH(ROWLOCK,UPDLOCK) SET borrowStatusId = 3
			FROM Borrow bow JOIN @DueList due ON bow.borrowId = due.borrowId AND bow.cid = due.cid
			WHERE borrowStatusId = 2

			-- 通知
			EXEC NotificationOverdueBorrow @OverDue = @DueList

			-- 回傳
			SELECT 1 ResultCode, N'回傳逾期資料' Message, cli.cName , cli.cAccount , col.title  
			FROM @DueList due 
			JOIN Client cli ON due.cid = cli.cid
			JOIN Book bok ON bok.bookId = due.bookid
			JOIN Collection col ON bok.collectionId = col.collectionId

			COMMIT
		END TRY

		BEGIN CATCH
			IF @@TRANCOUNT > 0 
			ROLLBACK
			SELECT 0 ResultCode, N'出現錯誤' + ERROR_MESSAGE() Message, NULL cName , NULL cAccount , NULL title  
		END CATCH
END
GO
--------------------------------------------------------------------------------
-- 書籍管理TVF
ALTER FUNCTION BookQueryResultView()
RETURNS TABLE
AS
RETURN(
	SELECT col.collectionId, col.isbn ,col.collectionImg , title, auth.author, col.collectionDesc, col.publisher, col.publishDate, typ.type,col.translator,lan.language, ISNULL(COUNT(*), 0) NumberOfBook
	FROM Collection col 
	JOIN Author auth ON col.authorId = auth.authorId
	JOIN Language lan ON col.languageId = lan.languageId
	JOIN Type typ ON col.typeId = typ.typeId
	LEFT JOIN Book bok ON col.collectionId = bok.collectionId
	GROUP BY col.collectionId,col.isbn, collectionImg, title, auth.author,col.collectionDesc, col.publisher, col.publishDate, typ.type,col.translator,lan.language
)
GO
--------------------------------------------------------------------------------
DROP PROC LateReturn
DROP PROC LateReturnOneToThree
DROP PROC NotificationAboutToExpire