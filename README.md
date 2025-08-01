【圖書館管理系統】（資展大專題，2025/06 ~ 2025/08）  

使用技術：C#、ASP.NET MVC、SQL Server、AJAX、JQuery、Bootstrap 。

功能介紹: 

對於讀者，可以查詢書籍、預約活動、預約書籍，個人的帳號可以看到借閱預約紀錄、最愛藏書等等，提升使用者對於借閱書籍體驗。

對於圖書館管理員，可以進行書籍的CRUD、借閱還書預約、預約管理、借閱查詢、排程(取書逾期、借閱逾期、即將逾期通知)，提升圖書館作業效率及自動化通知。

首頁
<img width="1261" height="617" alt="封面" src="https://github.com/user-attachments/assets/3c8eac71-96fb-4035-ac54-98e03f0e40a0" />
藏書
<img width="1253" height="617" alt="藏書" src="https://github.com/user-attachments/assets/aaf9b668-2587-430b-a3ab-7c315fa1101b" />
搜尋
<img width="1142" height="617" alt="搜尋" src="https://github.com/user-attachments/assets/270876ff-2699-45cc-9086-3635236a38ed" />
用戶首頁
<img width="1249" height="604" alt="用戶首頁" src="https://github.com/user-attachments/assets/e3cd5c8a-d800-4682-aa2b-ac50a35f5cca" />

管理者頁面
<img width="1895" height="891" alt="管理首頁" src="https://github.com/user-attachments/assets/a965056b-e594-4f0e-8b83-7c6ac9562696" />
書籍登陸: 可以在此新增書籍到資料庫中。
<img width="1197" height="660" alt="書籍登陸" src="https://github.com/user-attachments/assets/b605010d-7b57-4b0e-b082-ebf2e3870051" />

書籍管理: 可以搜尋書籍、修改、新增或刪除書籍編號。
<img width="1193" height="671" alt="書籍管理" src="https://github.com/user-attachments/assets/9854723e-55c6-471d-8efb-62fabbfa74c6" />
<img width="1200" height="669" alt="書籍管理_修改新增" src="https://github.com/user-attachments/assets/9a28f8d0-16df-482a-8997-753b622fb078" />

借閱模式: 讓圖書館管理員幫借閱者借閱書籍，之後也可以把這一個模組取出做成一個自助借書。
<img width="1196" height="646" alt="借閱模式" src="https://github.com/user-attachments/assets/a331d772-9362-44ff-b83e-094a2d247599" />
<img width="1256" height="615" alt="成功借閱" src="https://github.com/user-attachments/assets/28358464-ebe8-459d-aceb-ab32f7e6bf47" />

還書模式: 歸還書籍的功能，歸還後會找該本書有沒有下一位預約者，沒有，更改書籍狀態為"可借閱"，有的通知該名預約者，並把其預約紀錄修改成"可取書狀態"。
<img width="1193" height="646" alt="還書模式" src="https://github.com/user-attachments/assets/1b36627e-56dd-4108-8678-2b4bfad356c7" />
<img width="1260" height="626" alt="還書後" src="https://github.com/user-attachments/assets/1ea357f4-3fb3-4883-8b9d-11590cd9548f" />

預約模式: 除了個人預約，也可以在現場進行預約。
<img width="1268" height="596" alt="預約模式" src="https://github.com/user-attachments/assets/08486823-e821-4391-bb78-c674a1b22dca" />
<img width="1280" height="720" alt="預約成功" src="https://github.com/user-attachments/assets/bf8791e5-ea43-42a3-9d6e-6573f3f62d96" />

借閱查詢: 進行借閱查詢，如有需要也可以手動通知借閱者。
<img width="1193" height="676" alt="借閱查詢" src="https://github.com/user-attachments/assets/7436ab33-3ac6-457a-b4bf-dccf95fc29fe" />
<img width="1193" height="664" alt="借閱查詢_手動通知" src="https://github.com/user-attachments/assets/0735960b-b355-4dfa-9805-29a11ed6f9a9" />


預約管理: 除了個人可以取消預約，也可以在現場進行預約。
<img width="1197" height="653" alt="預約管理" src="https://github.com/user-attachments/assets/9f283498-46b3-4802-be9c-5db6f3a46498" />
<img width="1188" height="663" alt="預約管理_取消預約" src="https://github.com/user-attachments/assets/4dabdd07-86bb-4ea1-b9cd-b41e2357baa3" />

排程: 預約逾期、取書逾期、即將逾期通知，把各模組拆分封裝。
<img width="545" height="423" alt="排程程式碼" src="https://github.com/user-attachments/assets/093888e8-5502-464d-a6df-fb6369e5af63" />

