// #region 載入parital => 預約查詢、借閱查詢、借書模式、還書模式、預約模式、書籍登陸、書籍管理查詢
$(() => {
    $("#AppointmentQuery").on("click", initAppointmentPage)
    $("#BorrowQuery").on("click", BorrowQueryModule)
    $("#BorrowMode").on("click", BorrowModeMode)
    $("#ReturnMode").on("click", ReturnBookMode);
    $("#AppointmentMode").on("click", AppointmentMode);
    $("#dbConnect").on("click", DbContextText);
    $("#BooksAdded").on("click", BooksAdded);
    $("#BooksQuery").on("click", BooksQuery);
})
// #endregion

// #region 預約查詢&管理 Module
// 預約管理(搜尋欄)_初始頁載入
function initAppointmentPage() {
    $("#content-panel").load("/Backend/Manage/AppointmentQuery", () => {
        Appointment_queryEvent();
        $("#appointment_select").on("click", Appointment_queryEvent);
        $("#appointment_perPage, #appointment_orderDate ").on("change", Appointment_queryEvent)
        $("#appointment_clear").on("click", appointment_clearEvent);
    });
};
// 搜尋、排列、分頁
function Appointment_queryEvent() {
    $("#AppointmentContent").html(QueryWait);
    let value = $(this).data("page") || 1;
    let formData = $("#appointmenSearch").serialize() + `&page=${value}`;
    $.get("/Backend/Manage/AppointmentResult", formData, (result) => {
        $("#AppointmentContent").html(result);
        $(".page-link").on("click", Appointment_queryEvent);
        $(".NotificationBtn").on("click", CancelAppointmentBtn);
        $("#NotificationClear").on("click", NotificationClearBtn);
        $("#CancelBox").on("click", AppointmentNotificationClose);
        $("#NotificationSend").on("click", SendCancelAppointmentBtn);
    });
}
// 取消預約關閉按鈕
function AppointmentNotificationClose() {
    $('#notificationModal').modal("hide");
    $("#NotificationType").val("CancelNotification");
}
let booktitle;
let appointmentDate;

const formatDateISO = (date) => {
    return date.toLocaleDateString('en-CA');  
};
// 重新選擇
function CancelNotificationChange() {
    let cancelAppointmentText = `【取消預約通知】\n親愛的用戶您好，\n您所預約的書籍《 ${booktitle} 》\n已於 ${formatDateISO(new Date()) } 由本館管理員取消。\n取消原因： 考零分 。若您仍有借閱需求，歡迎重新進行預約。\n如有任何問題或需協助，敬請聯繫本館服務人員，我們將竭誠為您服務。感謝您的配合與理解！圖書館管理系統 敬上。`
    $("#NotificationTextarea").val(cancelAppointmentText);
}

// 取消預約按鈕
function CancelAppointmentBtn() {
    appointmentDate = $(this).closest("tr").find(".appointmentDate").text();
    booktitle = $(this).closest("tr").find(".booktitle").text();
    let appointmenId = $(this).closest("tr").find(".appointmentId").text();
    let cid = $(this).closest("tr").find(".appointmentcid").text();
    CancelNotificationChange();
    $("#NotificationType").on("click", CancelNotificationChange);
    $("#NotificationAppointmentId").val(appointmenId);
    $("#NotificationUser").val(cid);
}
// 送出取消預約
function SendCancelAppointmentBtn() {
    let CancelForm = $("#NotificationFom").serialize();
    $.ajax({
        method: "patch",
        url: "/Backend/Manage/CancelAppointment",
        data: CancelForm,
        success: (result) => {
            if (result == 0) {
                $('#notificationModal').modal("hide");
                swal("系統提示", "取消預約失敗", "error");
                return;
            }
            if (result == 1) {
                $('#notificationModal').modal("hide");
                swal("系統提示", "取消預約成功", "success");
                setTimeout(() => Appointment_queryEvent(), 1000)
                Appointment_queryEvent();
            }
        }
    })
    
    
}

// #endregion 預約查詢Module "END""

// #region 借閱查詢 Module
function BorrowQueryModule() { initBorrowPage(); }
// 借閱查詢(搜尋欄)_初始載入
function initBorrowPage() {
    $("#content-panel").load("/Backend/Manage/BorrowQuery", () => {
        borrow_queryEvent();
        $("#borrow_select").on("click", borrow_queryEvent);
        $(document).on("change", "#borrow_perPage,#borrow_date, #borrow_OrderDate, #borrow_orderBy", borrow_queryEvent);
        $("#borrow_clear").on("click", () => { $("#borrowForm")[0].reset(); });
    })
}
// 搜尋、分頁、排列
function borrow_queryEvent() {
    $("#BorrowContent").html("");
    $("#BorrowContent").html(QueryWait);
    let value = $(this).data("page") || 1;
    let borrowData = $("#borrowForm").serialize() + `&page=${value}`;
    $.get("/Backend/Manage/BorrowResult", borrowData, (result) => {
        $("#BorrowContent").html(result);
        $(".page-link").on("click", borrow_queryEvent);
        $(".NotificationBtn").on("click", NotificationBtn);
        $("#NotificationSend").on("click", NotificationMessageSend);
        $("#NotificationClear").on("click", NotificationClearBtn);
        $("#CancelBox").on("click", NotificationClose);
    })
}
// #endregion 借閱查詢Module END

// #region 借閱模式 Module
function BorrowModeMode() {
    $("#content-panel").load("/Backend/Manage/BorrowMode", () => {
        $("#borrowSend").on("click", BorrowModeSend);
        $("#borrwoMode_UserID").on("input", BorrowModeModeUserDynamic)
        $("#borrwoMode_BookCode").on("input", BorrowModeModeBookDynamic);
        $("#borrwoMode_CancelUserIDBtn").on("click", CancelBtnUser)
        $("#borrwoMode_CancelBookIdBtn").on("click", CancelBtnBook)
    });
}
// 動態搜尋 借閱者
function BorrowModeModeUserDynamic() {
    $("#BorrowModeSuccessContent").html("");
    let userId = $("#borrwoMode_UserID").val().trim();
    if (userId === "") { $("#BorrowModeSuccessContent").html(pleaseInputUserId); return; }
    $.get("/Backend/Manage/BorrowUserMessage", { userId: userId }, (result) => {
        if (result === false) { $("#BorrowModeSuccessContent").html(pleaseInputUserId); return; }
        $("#BorrowModeSuccessContent").html(result);
    })
}
// 動態搜尋 書本資訊
function BorrowModeModeBookDynamic() {
    //$("#BorrowModeSuccessContent").html("");
    let bookId = $("#borrwoMode_BookCode").val().trim();
    if (bookId === "") { $("#BorrowModeBook").html(pleaseInputBookId); return; }
    $.get("/Backend/Manage/BorrowBookMessage", { bookId: bookId }, (result) => {
        if (result === false) { $("#BorrowModeBook").html(pleaseInputBookId); return; }
        $("#BorrowModeBook").html(result);
    })
}
// 借閱書籍 發送POST
function BorrowModeSend() {
    let userId = $("#borrwoMode_UserID").val();
    let bookId = $("#borrwoMode_BookCode").val();
    if (userId === "" && bookId === "") { swal("系統提示","請輸入借閱者ID和書籍編號", "error"); return; }
    if (userId === "") { ("系統提示","請輸入借閱者編號","error"); return; }
    if (bookId === "") { swal("系統提示", "請輸入書籍編號", "error"); return; }
    let formData = $("#borrwoModeForm").serialize();
    let btnValue = $(this).val();
    if (btnValue === "borrow") {
        $.post("/Backend/Manage/BorrowSend", formData, (result) => {
            if (result === 0) { $("#BorrowModeSuccessContent").html(pleaseInputUserId); return; }
            $("#BorrowModeSuccessContent").html(result);
            BorrowModeModeBookDynamic();
        })
    }
}
function CancelBtnUser() {
    $(this).closest(".input-group").find(".form-control").val("");
    $("#BorrowModeSuccessContent").html("");
    $("#BorrowModeSuccessContent").html(pleaseInputUserId2);
}
function CancelBtnBook() {
    $(this).closest(".input-group").find(".form-control").val("");
    $("#BorrowModeBook").html(pleaseInputBookId2);
}
// #endregion 借書模式 END

// #region 還書模式 Module
function ReturnBookMode() {
    $("#content-panel").load("/Backend/Manage/ReturnBookMode", () => {
        $("#ReturnBookBtn").on("click", ReturnBookSend);
        $("#ReturnBook_CancelBookNumBtn").on("click", CancelBtn);
    })
}
// 還書送出
function ReturnBookSend() {
    let bookId = $("#ReturnBookCode").val();
    if (bookId === "") { swal("系統提示", "請輸入書籍編號", "error"); return; }
    let data = $("#ReturnBookIdForm").serialize();
    $.post("/Backend/Manage/ReturnBookSend", data, (result) => {
        //if (result.ResultCode === 4) { $("#ReturnBookContent").html(retrunFalse); return; }
        $("#ReturnBookContent").html(result);
        $("#ReturnBookCode").val("");
    })
}
// #endregion 還書模式 END Module

// #region 預約模式 Module
function AppointmentMode() {
    $("#content-panel").load("/Backend/Manage/AppointmentMode1", () => {
        $("#appointmentSend").on("click", AppointmentModeSend);
        $("#appointmentMode_KeyWord").on("input", AppointmentModeBookDynamic);
        $("#appointmentMode_CancelUserIdBtn ,#appointmentMode_CancelBookNumBtn").on("click", CancelBtn);
        $("#appointmentMode_CancelKeyWordBtn").on("click", CancelBtn_AppointVersion);
        $("#appointmentMode_KeyWord").on("click", BookQueryAutoComplete);
    })
}
// 關鍵字查詢
function AppointmentModeBookDynamic() {
    let keyWord = $("#appointmentMode_KeyWord").val();
    let state = $("#appointmentMode_status").val();
    let pageCount = $("#appointmentMode_perPage").val();
    let page = $(this).data("page") || 1;
    let obj = { keyWord: keyWord, state: state, pageCount: pageCount, page: page }
    if (keyWord === " ") {
        swal("系統提示", "請不要輸入空字串", "error");
        $("#appointmentMode_KeyWord").val("");
        return
    }
    if (keyWord === "") { $("#appointmentQueryBook").remove; $("#appointmentQueryBook").html(appointmentQueryBookHtml); return }
    $.get("/Backend/Manage/AppointmentMode1Query", obj, (result) => {
        if (result == 0) {
            $("#appointmentQueryBook").html(appointmentQueryBookHtml);
            $("#appointmentMode_status").val(state);
            appointmentOnChange();
            return;
        }
        $("#appointmentQueryBook").html(result);
        $("#appointmentMode_status").val(state);
        $("#appointmentMode_perPage").val(pageCount);
        $(".AppointmentMode_AddBookNumBtn").on("click", AppointmentModeAddBook);
        appointmentOnChange();
    });
};
// 狀態、頁數
function appointmentOnChange() {
    $("#appointmentMode_status").on("change", AppointmentModeBookDynamic);
    $("#appointmentMode_perPage").on("change", AppointmentModeBookDynamic);
    $(".page-link").on("click", AppointmentModeBookDynamic);
}
// 預約按鈕發送
function AppointmentModeSend() {
    let userId = $("#appointmentMode_UserID").val();
    let BookId = $("#appointmentMode_BookNumber").val();
    if (userId === "" && BookId === "") { swal("系統提示", "請輸入借閱者ID和書籍編號", "error"); return; }
    if (userId === "") { swal("系統提示", "請輸入借閱者", "error"); return; }
    if (BookId === "") { swal("系統提示", "請輸入書籍編號", "error"); return; }
    let formData = $("#appointmentModeForm").serialize();
    $.post("/Backend/Manage/AppointmentMode1Send", formData, (result) => {
        $("#appointmentSuccessContent").html(result);
        AppointmentModeBookDynamic();
    })
}
// 加入書籍編號到輸入框
function AppointmentModeAddBook() {
    let bookNumber = $(this).closest("tr").find("td").data("bookid");
    $("#appointmentMode_BookId").val(bookNumber);
}
// 關鍵字專屬清潔按鈕
function CancelBtn_AppointVersion() {
    $(this).closest(".input-group").find(".form-control").val("");
    $("#appointmentQueryBook").remove; $("#appointmentQueryBook").html(appointmentQueryBookHtml);
}
// #endregion 預約模式 Module END

// #region 書籍登陸
// 書籍登陸載入
function BooksAdded() {
    $("#content-panel").load("/Backend/Manage/BooksAdds", () => {
        $(".InputImg").on("change", BooksAdded_ShowImg);
        $(".BookAdd_Remove").on("click", BooksAdded_Remove);
        $("#BooksAdded_BtnSend").on("click", BooksAdded_BtnSend);
        $("#BooksAdded_BtnReset").on("click", BooksAdded_Reset);
        AuthorAutocomplete2();
        $("#BooksAdded_ISBM").on("input", FormatISBM);
        $(".BookAdd_Display").on("click", ClickImg);
        $(".BooksAddLoginTItle").on("click", BooksAddTestClickMe);
    })
}
// 圖片顯示
function BooksAdded_ShowImg() {
    if (this.files && this.files[0]) {
        var reader = new FileReader();
        reader.onload = (xian) => {$(".BookAdd_Display").attr("src", xian.target.result); }
        reader.readAsDataURL(this.files[0]);
        $('.BookAdd_Remove').prop('disabled', false); 
    }
}
// 移除圖片
function BooksAdded_Remove() {
    $(".BookAdd_Display").attr("src", "/images/InputTheBookImg.png");
    $(".BookAdd_Remove").prop("disabled", true);
    $(".InputImg").val("");
}
// 點擊圖片更換
function ClickImg() {
    $(".InputImg").trigger("click");
}
// 確定登入書籍
function BooksAdded_BtnSend() {
    let form = document.getElementById("BooksAdded_FormData");
    if (!form.checkValidity()) {
        form.classList.add("was-validated"); 
        return; 
    }
    swal("出版社字數超過上限請重新輸入!", "error");
    if ($("#BooksAdded_ISBM").val().length < 13) { swal("系統提示", "請輸入正確的13碼ISBM", "error"); $("#BooksAdded_ISBM").val(""); return; }
    if ($(".booktitle").val().length > 100) { swal("系統提示", "書籍名稱字數超過上限請重新輸入!", "error"); $(".booktitle").val("");return;}
    if ($(".authorName").val().length > 50) { swal("系統提示", "作者字數超過上限請重新輸入!", "error"); $(".authorName").val("");return;}
    if ($(".translator").val().length > 50) { swal("系統提示", "譯者字數超過上限請重新輸入!", "error"); $(".translator").val(""); return; }
    if ($(".pushier").val().length > 50) { swal("系統提示", "出版社字數超過上限請重新輸入!", "error"); $(".pushier").val(""); return; }
    var formdata = new FormData($("#BooksAdded_FormData")[0]);
    $.ajax({
        url: "/Backend/Manage/BooksCreate",
        type: "post",
        data: formdata,
        processData: false,
        contentType: false,
        success: (result) => {
            if (result.ResultCode === 1) {
                swal("系統提示", result.Message, "success");
                BooksAdded_Reset();
                $("#BooksAdded_FormData").removeClass("was-validated");
            }
            else {swal("系統提示", result.Message, "error");}
        }
    });
};
// 重置輸入
function BooksAdded_Reset() {
    $("#BooksAdded_FormData")[0].reset();
    $("#BooksAdded_Dec").text("");
    $("#BooksAdded_FormData").removeClass("was-validated");
    BooksAdded_Remove();
}
// 書籍登陸_測試資料
function BooksAddTestClickMe() {
    $("#BooksAdded_ISBM").val("303-957-13829-9-9");
    $("#BooksAdded_Title").val("動物農莊 Animal Farm: A Fairy Story");
    $("#BooksAdded_leng").val("3");
    $("#BooksAdded_Type").val("1");
    $("#BooksAdded_authorName").val("喬治．歐威爾 George Orwell");
    $("#BooksAdded_translator").val("楊煉");
    $("#BooksAdded_pushier").val("時報出版");
    $("#BooksAdded_puDate").val("2020-08-07");
    $("#BooksAdded_Dec").text("簡介20世紀百大英文小說譯成20餘種語言一個充滿政治寓言的童話故事所有動物一律平等，但有些動物比其他動物更平等。梅諾農莊的動物長久以來為主人辛勤工作，卻永遠吃不飽肚子，所有的勞動果實都被人類拿走了，這種悲慘的生活會一直持續到動物嚥下最後一口氣。有天，一隻德高望重的大白豬站出來提議大家奮起造反，推翻人類。之後某次時機到來，動物終於把醞釀已久的造反計畫付諸行動，他們奪下農莊，成為農莊主人。 雪球和拿破崙這兩隻年輕的豬出來帶領大家，並制定終極目標為「所有動物一律平等」的七戒給大家遵守。一開始，動物的食物充足，農莊也運作良好。但接著，群豬給自己留了額外的糧食，再接下來，雪球和拿破崙彼此為了最高領導人的位置互相出招競爭，一切似乎又回到了從前……。《動物農莊》是一部充滿政治寓言的童話故事，以動物社會諷刺人類世界的不公不義、荒唐可笑──勞動階級為了更平等的生活而揭竿起義，但當權力到手後，卻換了位置就換了腦袋，複製從前統治者的行徑，還變本加厲，讀來發人深省。")
}
// #endregion

// #region 書籍管理&查詢
// 書籍管理Partial
function BooksQuery() {
    $("#content-panel").load("/Backend/Manage/BooksQuerys", () => {
        EnteryBookQuery();
        $("#book_select").on("click", EnteryBookQuery);
        $("#book_ISBN").on("input", FormatISBM);
        $("#book_clear").on("click", () => { $("#BookForm")[0].reset() });
        $("#book_KeyWord").on("click", BookQueryAutoComplete);
    })
}
// 書籍管理搜尋
function EnteryBookQuery() {
    $("#BookContent").html("");
    $("#BookContent").html(QueryWait);
    let page = $(this).data("page") || 1;
    let formDate = $("#BookForm").serialize() + `&page=${page}`;
    $.get("/Backend/Manage/BooksQueryResult", formDate, (result) => {
        $("#BookContent").html(result);
        $(".page-link").on("click", EnteryBookQuery);
        $("#borrow_perPage").on("change", EnteryBookQuery);
        $("#borrow_orderBy").on("change", EnteryBookQuery);
        $(".stopCollapse").on("click", function (e) { e.stopPropagation(); });
    });
}
// #region Collection 編輯修改
// 點擊藏書修改時，儲存的資料，用於取消時返回
const BookContextHTMLTmp = [];
const BookImgTmp = [];
// 藏書修改Btn
function BookQueryEdit(xian) {
    let thisBtn = $(xian);
    let thisRow = thisBtn.closest("tr");
    let collectionId = thisRow.find(".bookImg").data("collectionid");

    let rowClone = "";
    thisRow.children("td").slice(0, 5).each(function () {
        rowClone += $(this).prop("outerHTML");
    })

    let index = BookContextHTMLTmp.findIndex(x => x.collectionId === collectionId);
    if (index !== -1) { BookContextHTMLTmp[index].rowClone = rowClone; }
    else { BookContextHTMLTmp.push({ collectionId, rowClone }); }

    // 儲存圖片
    let bookImgSrc = thisRow.find(".bookImgSide").attr("src") !== undefined ? thisRow.find(".bookImgSide").attr("src") :  thisRow.find(".BookAdd_Display").attr("src");

    let imgindex = BookImgTmp.findIndex(x => x.collectionId === collectionId);
    if (imgindex !== -1) { BookImgTmp[imgindex].bookImgSrc = bookImgSrc }
    else { BookImgTmp.push({ collectionId, bookImgSrc }); }

    let bookISBM = thisRow.find(".bookISBM").text().trim();
    let bookTitle = thisRow.find(".bookTitle").text().trim();
    let bookAuthor = thisRow.find(".bookAuthor").text().trim();
    let bookTranslator = thisRow.find(".bookTranslator").text().trim();
    let bookType = thisRow.find(".bookType").text().trim();
    let bookLang = thisRow.find(".bookLang").text().trim();
    let bookPublisher = thisRow.find(".bookPublisher").text().trim();
    let bookPublishDate = thisRow.find(".bookPublishDate").text().trim();
    let bookDesc = thisRow.find(".bookDesc").text().trim();

    thisRow.find(".bookTranslatorTr").removeClass("d-none");
    thisRow.find(".bookImg").data("collapse-enabled", false);
    thisRow.find(".bookStored").removeClass("d-none");
    thisRow.find(".bookCancle").removeClass("d-none");
    thisRow.find(".bookEdit").addClass("d-none");
    thisRow.find(".bookAdd").addClass("d-none");
    // 圖片
    thisRow.find(".bookImg").html(`
        <input type="file" class="form-control form-control fw-bold fs-3 d-none InputImg" />
        <img src="" style="width: 300px; height: 450px" alt="無圖片" class="border border-4 BookAdd_Display" />
        <button type="button" class="btn btn-success bg-opacity-50 fs-4 fw-bold bookImgReset w-25 mt-3">復原圖片</button> `)

    thisRow.find(".bookISBM").html(`<input class="form-control form-control fw-bold fs-3" oninput= "FormatISBM2(this)"  value="${bookISBM}" />`);
    thisRow.find(".bookTitle").html(`<input class="form-control form-control fw-bold fs-3" value="${bookTitle}"/>`);
    thisRow.find(".bookAuthor").html(`<input class="form-control form-control fw-bold fs-3 authorName" value="${bookAuthor}" /><input type="number" class="d-none authorId"  value="0"/>`);
    thisRow.find(".bookTranslator").html(`<input class="form-control form-control fw-bold fs-3" value="${bookTranslator}"/>`);
    thisRow.find(".bookPublisher").html(`<input class="form-control form-control fw-bold fs-3" value="${bookPublisher}"/>`);
    thisRow.find(".bookPublishDate").html(`<input class="form-control form-control fw-bold fs-3" type="date" value="${bookPublishDate.replaceAll('/', '-')}"/>`);
    thisRow.find(".bookDesc").html(`<textarea class="bg-light border rounded p-2 my-0 fw-bold fs-3"  style="white-space: pre-wrap;height: 550px; width:100%"/>${bookDesc}</textarea>`);

    let imgindexRealality = BookImgTmp.findIndex(x => x.collectionId === collectionId);
    thisRow.find(".BookAdd_Display").attr("src", BookImgTmp[imgindexRealality].bookImgSrc);
    thisRow.find(".bookImgReset").on("click", BookImgResetBtn);
    thisRow.find(".BookAdd_Display").on("click", () => {
        thisRow.find(".InputImg").trigger("click");
    });
    thisRow.find(".InputImg").on("change", BooksAdded_ShowImg2);
    AuthorAutocomplete2();
    BookType(thisRow, bookType);
    BookLanguage(thisRow, bookLang);
}

//藏書儲存BTN
function BookStoredBtn(xian) {
    let thisRow = $(xian).closest("tr");

    let collectionId = thisRow.find(".bookImg").data("collectionid");
    let newbookISBM = thisRow.find(".bookISBM input").val();
    let newbookTitle = thisRow.find(".bookTitle input").val();
    let newbookAuthor = thisRow.find(".bookAuthor input").val();
    let newbookAuthorId = thisRow.find(".bookAuthor").find(".authorId").val();
    let newbookTranslator = thisRow.find(".bookTranslator input").val();
    let newbookLangid = thisRow.find(".bookLang select").val();
    let newbookLang = thisRow.find(".bookLang select option:selected").text();
    let newbookTypeid = thisRow.find(".bookType select").val();
    let newbookType = thisRow.find(".bookType select option:selected").text();
    let newbookPublisher = thisRow.find(".bookPublisher input").val();
    let newbookPublishDate = thisRow.find(".bookPublishDate input").val();
    let newbookDesc = thisRow.find(".bookDesc textarea").val();
    let newBookImg = thisRow.find(".BookAdd_Display").prop("src");

    swal("系統提示", "出版社字數超過上限請重新輸入!", "error");
    if (newbookISBM.length < 13) { swal("系統提示", "請輸入正確的13碼 ISBM", "error"); thisRow.find(".bookISBM input").val(""); return; }
    if (newbookTitle.length > 100) { swal("系統提示", "書籍名稱字數超過上限請重新輸入!", "error"); thisRow.find(".bookTitle input").val(""); return; }
    if (newbookAuthor.length > 50) { swal("系統提示", "作者字數超過上限請重新輸入!", "error"); thisRow.find(".bookAuthor input").val(""); return; }
    if (newbookTranslator.length > 50) { swal("系統提示", "譯者字數超過上限請重新輸入!", "error"); thisRow.find(".bookTranslator input").val(""); return; }
    if (newbookPublisher.length > 50) { swal("系統提示", "出版社字數超過上限請重新輸入!", "error"); thisRow.find(".bookPublisher input").val(""); return; }
    let data = {
        CollectionId: collectionId,
        Title: newbookTitle,
        CollectionDesc: newbookDesc,
        TypeId: newbookTypeid,
        Translator: newbookTranslator,
        Publisher: newbookPublisher,
        LanguageId: newbookLangid,
        Isbn: newbookISBM,
        PublishDate: newbookPublishDate,
        AuthorId: newbookAuthorId,
        Author: newbookAuthor,
        CollectionImg: newBookImg
    }
    ////送出
    $.ajax({
        method: "patch",
        url: "/Backend/Manage/BookQueryEdit",
        contentType: "application/json",
        data: JSON.stringify(data),
        success: (result) => {
            swal("系統提示", result.Message, "error");
            if (result.ResultCode === 0) { swal("系統提示", result.Message, "error"); return; }
            swal("系統提示", result.Message, "success");
            thisRow.find(".bookISBM").html(newbookISBM);
            thisRow.find(".bookTitle").html(newbookTitle);
            thisRow.find(".bookAuthor").html(newbookAuthor);
            if (newbookTranslator.trim() !== "") { thisRow.find(".bookTranslator").html(newbookTranslator); }
            else (thisRow.find(".bookTranslatorTr").addClass("d-none"))
            thisRow.find(".bookType").html(newbookType);
            thisRow.find(".bookLang").html(newbookLang);
            thisRow.find(".bookPublisher").html(newbookPublisher);
            thisRow.find(".bookPublishDate").html(newbookPublishDate);
            if (newbookDesc.length > 0) {thisRow.find(".bookDesc").html(`<pre class="bg-light border rounded p-2 my-0" style="white-space: pre-wrap; max-height: 420px; width: 100%; ">${newbookDesc}</pre>`); }
            else {thisRow.find(".bookDesc").html(`-`); }
            thisRow.find(".bookStored").addClass("d-none");
            thisRow.find(".bookCancle").addClass("d-none");
            thisRow.find(".bookEdit").removeClass("d-none");
            thisRow.find(".bookImg").html(`<img src="${newBookImg}" style="width: 300px; height: 450px" alt="無圖片" class="border border-4 BookAdd_Display" />`)
            thisRow.find(".bookImg").data("collapse-enabled", true);
        },
        Error: (err) => {
            swal("發生錯誤", err, "error");
        }
    })
}

// 藏書取消Btn
function BookCancelBtn(xian) {
    let thisRow = $(xian).closest("tr");
    let collectionId = thisRow.find(".bookImg").data("collectionid");
    let index = BookContextHTMLTmp.findIndex(x => x.collectionId === collectionId);
    let rowClone = BookContextHTMLTmp[index].rowClone;
    if (index !== -1) { thisRow.html(rowClone) }
    thisRow.find(".bookStored").addClass("d-none");
    thisRow.find(".bookCancle").addClass("d-none");
    thisRow.find(".bookEdit").removeClass("d-none");
    thisRow.find(".bookAdd").removeClass("d-none");
    thisRow.find(".bookImg").data("collapse-enabled", true);
}

// 圖片復原BTN
function BookImgResetBtn() {
    let thisRow = $(this).closest("tr");
    let collectionId = thisRow.find(".bookImg").data("collectionid");
    let index = BookImgTmp.findIndex(x => x.collectionId === collectionId);
    thisRow.find(".BookAdd_Display").attr("src", BookImgTmp[index].bookImgSrc);
}
// 圖片變更BTN
function BooksAdded_ShowImg2() {
    console.log("變變");
    let thisRow = $(this).closest("tr");
    if (this.files && this.files[0]) {
        var reader = new FileReader();
        reader.onload = (andy) => { thisRow.find(".BookAdd_Display").attr("src", andy.target.result); }
        reader.readAsDataURL(this.files[0]);
    }
}
//  #endregion

// #region BookCode 新增
// 點擊圖片 顯示該書的bookcode
function DisplayBookCode(xian) {
    const pickkRow = $(xian); 
    let collecionId = pickkRow.data("collectionid");
    let enbled = pickkRow.data("collapse-enabled");
    let collapseId = $(`#collapse_${collecionId}`);
    if (!enbled) { return; }
    if (!pickkRow.data("loaded")) {
        $.ajax({
            type: "get",
            url: "/Backend/Manage/BookQueryBookCode",
            data: { collecionId1: collecionId },
            success: (result) => {
                collapseId.html(result);
                pickkRow.data("loaded", true);
                $(".bookAdd").on("click", InvokeBookQueryAddBook);
                $(".bookAddMuti").on("click", BookQuerAddBookCodeCircle);
                $(".bookCodeDelete").on("click", BookQueryQueryDeleteBookCode);
                $(".bookLock").on("click", BookQueryUnLock);
                $(".bookUnlock").on("click", BookQUeryLock);
            },
            error: (err) => {
                swal("發生錯誤", err, "error");
            }
        });
    }
    const bsCollapse = bootstrap.Collapse.getOrCreateInstance(collapseId[0]);
    bsCollapse.toggle();
}
// 新增書籍 觸發按鈕
function InvokeBookQueryAddBook() {
    let thisRow = $(this).closest("tr");
    let amount = thisRow.find(".bookAddAmount").val();
    for (let x = 1; x <= amount; x++) {
        thisRow.find(".bookAddMuti").trigger("click");
    }
    thisRow.find(".bookAddAmount").val("1");
}
// 新增書籍 間接按鈕
function BookQuerAddBookCodeCircle() {
    let thisRow = $(this).closest("tr");
    let closeTr = thisRow.prevAll("tr").first();
    BookQueryAddBookCode(thisRow, closeTr);
}
// 新增 BookCode
function BookQueryAddBookCode(row, closetr) {
    let thisRow = $(row);
    let closeTr = $(closetr);
    // BookCode新增....
    let collectionid = closeTr.data("collecitonId");
    let BookCode = closeTr.find(".bookcodeId").text();
    let beforeBookCodeString;
    let afterBookCodeNumber;
    if (closeTr.find(".bookcodeId input").val() === undefined) {
        let BookCodeLength = BookCode.length;
        let afterBookCode = BookCode.slice(BookCodeLength - 3, BookCodeLength);
        beforeBookCodeString = BookCode.slice(0, BookCodeLength - 4);
        afterBookCodeNumber = (parseInt(afterBookCode) + 1).toString().padStart(4, '0');
    }
    else {
        let afterBookCode = closeTr.find(".bookcodeinput").val();
        beforeBookCodeString = BookCode;
        afterBookCodeNumber = (parseInt(afterBookCode.toString()) + 1).toString().padStart(4, '0');;
    }
    
    let bookcoderow1=`
    <tr class="fw-bold fs-3 align-middle" data-colleciton-Id="${collectionid}">
        <td class="bookcodeId d-flex justify-content-center align-items-center" style="height:76px">${beforeBookCodeString}<input class="form-control form-control fw-bold fs-3 text-center mx-1 bookcodeinput" onKeyPress="if(this.value.length==4) return false;" type="number" value="${afterBookCodeNumber}"  style="width:120px"></td>
        <td class="bookAddDate"></td>
        <td class="align-middle text-light text-center bookStatus">
            <select id="bookCode_Status" name="bookCode_Status" class="form-select form-select-sm fs-3 fw-bold mx-auto text-center bookCodeStatus">
                <option value="1" class="fw-bold">館內</option>
                <option value="3">無法借閱</option>
            </select>
        </td>
        <td class="bookBtn"><button type="button" class="btn btn-success btn-lg fw-bold fs-3 bookcheck mx-2" onclick="BookQueryCheckAddsBookcode(this)">確認</button><button type="button" class="btn btn-secondary btn-lg fw-bold fs-3 bookcheck" onclick="BookQueryCheckCancelBookcode(this)" >取消</button></td>
    </tr>`
    $(bookcoderow1).insertBefore(thisRow);
}
//  確認新增 BookCode
function BookQueryCheckAddsBookcode(xian) {
    let thisRow = $(xian).closest("tr");
    let bookCodeBefore = thisRow.find(".bookcodeId").text();
    let bookCodeAfter = thisRow.find(".bookcodeId input").val().toString().padStart(4, '0');
    let BookCode = bookCodeBefore + bookCodeAfter;
    let newDate = new Date();
    let backendDateTime = newDate.toISOString().slice(0, 19);

    //// 時間轉換 =_=
    let datePart = newDate.toLocaleDateString("zh-TW"); // "2025/07/13"
    let hour = newDate.getHours();
    let ampm = hour < 12 ? "上午" : "下午";
    let displayHour = (hour % 12 || 12).toString().padStart(2, "0");
    let minutes = newDate.getMinutes().toString().padStart(2, "0");
    let seconds = newDate.getSeconds().toString().padStart(2, "0");
    let displayDate = `${datePart} ${ampm} ${displayHour}:${minutes}:${seconds}`;

    $(thisRow).find(".bookAddDate").text(displayDate);
    
    const data = {
        CollectionId: parseInt(thisRow.attr("data-colleciton-Id")),
        BookCode: BookCode,
        AccessionDate: backendDateTime,
        BookStatusId : parseInt(thisRow.find(".bookCodeStatus").val())
    }
    $.ajax({
        type: "post",
        url: "/Backend/Manage/BookQueryAddsBookCode",
        contentType: "application/json",
        data: JSON.stringify(data),
        success: (result) => {
            if (result.ResultCode === 0) {
                swal("系統提示", result.Message, "error");
            }
            swal("系統提示", result.Message, "success");
            if (data.BookStatusId === 1) {
                //thisRow.find(".bookStatus").addClass("bg-success")
                thisRow.find(".bookStatus").attr("style", "background-color: var(--c-brown3-100)");
                thisRow.find(".bookStatus").html("館內")
            }
            else {
                //thisRow.find(".bookStatus").addClass("bg-secondary")
                thisRow.find(".bookStatus").attr("style", "background-color: var(--c-black-075)");
                thisRow.find(".bookStatus").html("無法借閱");
            }
            thisRow.find(".bookcodeId").html(BookCode);
            thisRow.find(".bookBtn").html(`<button type="button" class="btn btn-primary btn-lg fw-bold fs-3 bookCodeEdit mx-2" onclick="BookQueryEditBookCode(this)">修改</button><button type="button" class="btn btn-lg fw-bold fs-3 bookCodeDelete bookCodelock text-white disabled" onclick="BookQueryQueryDeleteBookCode(event, this)"  style="background-color: rgba(166, 30, 53, 1)">刪除</button>`);
        },
        error: (err) => {
            swal("發生錯誤", err, "error");
        }
    });
}
//  取消 BookCode
function BookQueryCheckCancelBookcode(xian) {
    $(xian).closest("tr[data-colleciton-Id]").remove();
}
// #endregion

// #region BookCode 狀態更改  
// BookCodeStatusArrray
let tmpBookCodeStaus = [];
// 書籍狀態_修改BookCode_BTN
function BookQueryEditBookCode(xian) {
    let bookStatusSelect;
    let thisRow = $(xian).closest("tr");
    let BookCode = thisRow.find(".bookcodeId").text().trim();
    let btnHtml = `<button type="button" class="btn btn-success btn-lg fw-bold fs-3 bookcheck mx-2" onclick="BookQueryCheckChangeStatus(this)">確認</button><button type="button" class="btn btn-secondary btn-lg fw-bold fs-3 bookcheck" onclick="BookQueryCancelChangeStatus(this)">取消</button>`;
    let thisBookStatus = thisRow.find(".bookStatus").text().trim();
    if (thisBookStatus == "無法借閱") { bookStatusSelect = `<select id="bookCode_Status" name="bookCode_Status" class="form-select form-select-sm fs-3 fw-bold mx-auto text-center bookCodeStatus" ><option value="1" class="fw-bold">館內</option> <option value="3" selected>無法借閱</option></select>`; }
    else { bookStatusSelect = `<select id="bookCode_Status" name="bookCode_Status" class="form-select form-select-sm fs-3 fw-bold mx-auto text-center bookCodeStatus" ><option value="1" class="fw-bold">館內</option> <option value="3">無法借閱</option></select>`; }
    thisRow.find(".bookStatus").html(bookStatusSelect);
    thisRow.find(".bookBtn").html(btnHtml);
    let BookStatusId = thisRow.find(".bookCodeStatus").val();
    const index = tmpBookCodeStaus.findIndex(x => x.BookCode === BookCode);
    if (index !== -1) {
        tmpBookCodeStaus[index].BookStatusId = BookStatusId
    }
    else {
        tmpBookCodeStaus.push({ BookCode, BookStatusId })
    }
}
// 書籍狀態_確認BookCode_BTN
function BookQueryCheckChangeStatus(xian) {
    let thisRow = $(xian).closest("tr");
    let collectionid = parseInt(thisRow.data("collecitonId"))
    let BookCode = thisRow.find(".bookcodeId").text();

    let bookCodeStatus = thisRow.find(".bookCodeStatus").val();
    let data = {
        CollectionId: collectionid,
        BookCode: BookCode,
        BookStatusId: bookCodeStatus
    }
    $.ajax({
        type : "patch",
        url: "/Backend/Manage/BookQueryEditBookStatus",
        contentType: "application/json",
        data: JSON.stringify(data),
        success: (result) => {
            if (result.ResultCode === 0) {
                swal("系統提示", result.Message, "error");
            }
            swal("系統提示", result.Message, "success");
            if (bookCodeStatus === "1") {
                thisRow.find(".bookStatus").attr("style", "background-color: var(--c-brown3-100)");
                thisRow.find(".bookStatus").html("館內")
            }
            else {
                thisRow.find(".bookStatus").attr("style", "background-color: var(--c-black-075)");
                thisRow.find(".bookStatus").html("無法借閱");
            }
            thisRow.find(".bookcodeId").html(BookCode);
            thisRow.find(".bookBtn").html(`<button type="button" class="btn btn-primary btn-lg fw-bold fs-3 bookCodeEdit mx-2" onclick="BookQueryEditBookCode(this)">修改</button><button type="button" class="btn btn-lg fw-bold fs-3 bookCodeDelete bookCodelock text-white disabled" onclick="BookQueryQueryDeleteBookCode(event, this)" style="background-color: rgba(166, 30, 53, 1)">刪除</button>`);
        },
        error: (err) => {
            swal("發生錯誤", err, "error");
        }
    })
}
// 書籍狀態_取消BookCode_BTN
function BookQueryCancelChangeStatus(xian) {
    let thisRow = $(xian).closest("tr");
    let BookCode = thisRow.find(".bookcodeId").text().trim();
    const index = tmpBookCodeStaus.findIndex(x => x.BookCode === BookCode);
    let BookStatusId = tmpBookCodeStaus[index].BookStatusId;
    if (BookStatusId === "1") {
        thisRow.find(".bookStatus").attr("style", "background-color: var(--c-brown3-100)");
        thisRow.find(".bookStatus").html("館內")
    }
    else {
        thisRow.find(".bookStatus").attr("style", "background-color: var(--c-black-075)");
        thisRow.find(".bookStatus").html("無法借閱");
    }
    thisRow.find(".bookcodeId").html(BookCode);
    thisRow.find(".bookBtn").html(`<button type="button" class="btn btn-primary btn-lg fw-bold fs-3 bookCodeEdit mx-2" onclick="BookQueryEditBookCode(this)">修改</button><button type="button" class="btn btn-lg fw-bold fs-3 bookCodeDelete bookCodelock text-white disabled" onclick="BookQueryQueryDeleteBookCode(event, this)" style="background-color: rgba(166, 30, 53, 1)">刪除</button>`);
}
// 刪除書籍_BookCode
function BookQueryQueryDeleteBookCode(xian) {
    let thisRow = $(xian.target).closest("tr");
    let bookCodeid = thisRow.find(".bookcodeId").text().trim();
    $.ajax({
        type: "delete",
        url: "/Backend/Manage/BookQueryDeleteBookCode",
        data: { bookCodeid: bookCodeid },
        success: (result) => {
            if (result.ResultCode === 0) {
                swal("系統提示", result.Message, "error");
                return;
            }
            swal("系統提示", result.Message, "info");
            $(xian.target).closest("tr[data-colleciton-Id]").remove();
        },
    })
}
// 解鎖Lock_BookCode
function BookQueryUnLock() {
    let thisRow = $(this).closest("tr");
    let thisTbody = thisRow.closest("thead").next();
    thisRow.find(".bookUnlock").removeClass("d-none");
    thisRow.find(".bookLock").addClass("d-none");
    thisTbody.find(".bookCodelock").removeClass("disabled");
}
// 上鎖Lock_BookCode
function BookQUeryLock() {
    let thisRow = $(this).closest("tr");
    let thisTbody = thisRow.closest("thead").siblings("tbody");
    thisRow.find(".bookUnlock").addClass("d-none");
    thisRow.find(".bookLock").removeClass("d-none");
    thisTbody.find(".bookCodelock").addClass("disabled");
}
// #endregion

// #endregion

// #region 通用函數
let TempBookName;
let DueDate;
// 按鈕清除 
function CancelBtn() {
    $(this).closest(".input-group").find(".form-control").val("");
}
// 清空搜尋資料
function appointment_clearEvent() {
    $("#appointmenSearch")[0].reset();
}
// 點擊通知
function NotificationBtn() {
    $("#NotificationType").on("change", ChageNotificationType);
    TempBookName = $(this).closest("tr").find(".BorrowBookTitle").text();
    DueDate = $(this).closest("tr").find(".BorrowDueDate").text();
    ChageNotificationType();
    let recipientId = $(this).closest("tr").find(".NotificationUserid").text();
    let recipientName = $(this).closest("tr").find(".NotificationUserName").text();
    let typeinput = $("#NotificationType").val();
    $("#NotificationInput").val(typeinput);
    $("#NotificationId").val(recipientId);
    $("#NotificationName").val(recipientName);
};
// 預設通知內容
function ChageNotificationType() {
    let NotificationType = $("#NotificationType").val();
    let UpcomingExpirationNoticeText = `親愛的用戶您好，\n您所借閱的書籍《${TempBookName}》\n即將於  ${DueDate.replaceAll('/', '-')}  到期 \n 請您於期限前歸還，謝謝。\n圖書館管理系統 敬上。`;
    let ExpirationNoticeWarningText = `親愛的用戶您好，\n您所借閱的書籍《${TempBookName}》已逾期\n請儘速歸還並聯繫館方補辦相關事宜，謝謝您的配合。\n圖書館管理系統 敬上。`;
    if (NotificationType === "即將到期通知") { $("#NotificationTextarea").val(UpcomingExpirationNoticeText); }
    if (NotificationType === "逾期警告通知") { $("#NotificationTextarea").val(ExpirationNoticeWarningText); }
    if (NotificationType === "圖書館通知") { $("#NotificationTextarea").val(""); }
}
// 通知送出按鈕
function NotificationMessageSend() {
    let myform = $("#NotificationFom").serialize();
    $.post("/Backend/Manage/Notification", myform, (result) => {
        if (result === 1) {
            swal("系統提示", "成功送出!!", "success");
        }
        else if (result === 0) {
            swal("系統提示", "送出失敗....", "error");
        }
        else {
            swal("系統提示", result, "info");
        }
        NotificationClose();
    })
};
// 清除按鈕
function NotificationClearBtn() {
    $("#NotificationTextarea").val("");
}
// 關閉視窗
function NotificationClose() {
    $('#notificationModal').modal("hide");
    $("#NotificationTextarea").val("");
    $("#NotificationType").val("即將到期通知");
}
// ISBN Fomart
function FormatISBM() {
    let val = $(this).val().replace(/[^0-9]/g, "");
    if (val.length > 12) {
        val = `${val.slice(0, 3)}-${val.slice(3, 6)}-${val.slice(6, 11)}-${val.slice(11, 12)}-${val.slice(12, 13)}`;
    }
    $(this).val(val);
}
function FormatISBM2(inputElement) {
    let val = $(inputElement).val().replace(/[^0-9]/g, "");
    if (val.length > 12) {
        val = `${val.slice(0, 3)}-${val.slice(3, 6)}-${val.slice(6, 11)}-${val.slice(11, 12)}-${val.slice(12, 13)}`;
    }
    $(inputElement).val(val);
}
// 關鍵字
function BookQueryAutoComplete() {
    $(this).autocomplete({
        minLength: 1,
        source: (request, response) => {
            $.ajax({
                method: "get",
                url: "/Backend/Manage/KeyWordAuthorSearch",
                data: { keyword: request.term },
                success: (data) => {
                    const result = data.map(x => ({
                        label: x.Label,
                        value: x.Value
                    }));
                    response(result);
                }
            })
        },
        select: (event, ui) => {
            $(this).val(ui.item.value);
            let autoId = $(this).attr("id");
            if (autoId === "appointmentMode_KeyWord") { AppointmentModeBookDynamic(); }
            return false;
        }
    })
}
// 作者關鍵字Autocomplete
function AuthorAutocomplete2() {
    $(".authorName").autocomplete({
        minLength: 1,
        source: function (request, response) {
            $.ajax({
                method: "get",
                url: "/Backend/Manage/AuthorSearch",
                data: { authorLike: request.term },
                success: function (data) {
                    const result = data.map(x => ({
                        label: x.Author1,
                        value: x.AuthorId
                    }));
                    response(result);
                }
            })
        },
        focus: function (event, ui) {
            event.preventDefault();
            $(this).val(ui.item.label);
        },
        select: function (event, ui) {
            $(".authorName").val(ui.item.label);
            $(".authorId").val(ui.item.value);
            return false;
        }
    })
}
// 書籍類型
function BookType(row, type) {
    $.ajax({
        method: "get",
        url: "/Backend/Manage/TypeSearch",
        data: { type : type },
        success: (result) => {
            $(row).find(".bookType").html(result);
        },
        Error: (err) => {
            swal("發生錯誤", err, "error");
        }
    })
}
// 書籍語言
function BookLanguage(row, language) {
    $.ajax({
        method: "get",
        url: "/Backend/Manage/LanguageSearch",
        data: { language: language },
        success: (result) => {
            $(row).find(".bookLang").html(result);
        },
        Error: (err) => { swal("發生錯誤", err, "error"); }
    })
}
// 時區
const formatter = new Intl.DateTimeFormat("zh-TW", {
    year: "numeric",
    month: "numeric",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    hour12: true,
    timeZone: "Asia/Taipei"
});
// #endregion

// #region 可用的HTML
// 預約模式_顯示欄位
let appointmentQueryBookHtml = `
        <div class="d-flex justify-content-between align-items-end">
            <div class="d-flex flex-wrap gap-3 justify-content-end fs-3">
                <div>
                    <label for="appointmentMode_status" class="form-label mb-0 fw-bold">借閱狀態</label>
                    <select id="appointmentMode_status" name="appointmentMode_status" class="form-select form-select-sm fs-3 fw-bold">
                        <option value="ALL" selected>全部</option>
                        <option value="IsLent">可借閱</option>
                        <option value="Available">借閱中</option>
                    </select>
                </div>
                <div>
                    <label for="appointmentMode_perPage" class="form-label mb-0 fw-bold">筆數</label>
                    <select id="appointmentMode_perPage" name="appointmentMode_perPage" valus="10" class="form-select form-select-sm fs-3 fw-bold">
                        <option value="10" selected>10</option>
                        <option value="20">20</option>
                        <option value="30">30</option>
                    </select>
                </div>
            </div>
        </div>
        <div>
            <div class=" fs-3 d-flex flex-wrap gap-3 justify-content-start"></div>
            <table class="table mt-2 table-hover" style="width: 100%; table-layout: fixed;">
                <thead class="fs-3 fw-bold   table-success">
                    <tr>
                        <th scope="col" style="width:500px">書籍名稱</th>
                        <th scope="col" style="width:200px">作者</th>
                        <th scope="col" class="text-center" style="width:150px">借閱狀態</th>
                        <th scope="col" class="text-center" style="width:150px">預約人數</th>
                        <th scope="col" class="text-center" style="width:100px">操作</th>
                    </tr>
                </thead>
                <tbody>
                </tbody>
            </table>
            <div class="container align-middle">
                <h1 class="text-danger mt-1 fs-1 fw-bold">查無書籍</h1>
            </div>
        </div>`;

let pleaseInputUserId = `<div class="alert alert-danger fs-1 text-center">該名借閱者不存在，請重新輸入</div>`;
let pleaseInputBookId = `<div class="row col-12"><div class="alert alert-danger fs-1 mt-5 text-center">該籍書籍不存在，請重新輸入</div></div>`;
let pleaseInputUserId2 = `<div class="alert alert-danger fs-1 text-center">請輸入借閱者ID</div>`
let pleaseInputBookId2 = `<div class="row col-12"><div class="alert alert-danger fs-1 mt-5 text-center">請輸入書籍編號</div> </div>`;
let QueryWait = `
<div class="container text-center mt-5 ">
    <div class="spinner-border text-success fs-1 mt-5" style="width: 10rem; height: 10rem; role="status">
      <span class="visually-hidden">Loading...</span>
    </div>
</div>
`;
let QueryFalse = `<div class="alert alert-danger fs-1">查無資料</div>`;
let retrunFalse = `<div class="alert alert-danger fs-1">輸入錯誤，請重新輸入!</div>`;
// #endregion

//#region Db連線
function DbContextText() {
    console.log("DB連線測試");
    $.post("/Backend/Manage/TestDbContext", (restult) => {
        if (restult === true) { alert("連線成功!!!"); }
        else {alert("連線失敗...") }
    })
}
//#endregion