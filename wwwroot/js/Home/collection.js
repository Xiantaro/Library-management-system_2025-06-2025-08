const pageCount = $(".pageClass").length;
let pageLimit = 0;
let paginCount = 0;
let paginIndex = 1;
let clickCount = 0;

chkStatus = () => {
    const pageS = clickCount * pageLimit + 1;
    const pageE = Math.min((clickCount + 1) * pageLimit, pageCount);

    paginCount = Math.ceil(pageCount / pageLimit);

    for (let i = 1; i <= pageCount; i++) {
        $(`#btn_${i}`).addClass("d-none")
        $(`#man_${i}`).addClass("d-none")
    }

    for (let i = pageS; i <= pageE; i++) {
        $(`#btn_${i}`).removeClass("d-none")
        $(`#man_${i}`).removeClass("d-none")
    }

    if (pageCount <= pageLimit) {
        $("#btn_prev").addClass("clickClass")
        $("#btn_next").addClass("clickClass")
    }
    else {
        $("#btn_prev").toggleClass("clickClass", clickCount === 0)
        $("#btn_next").toggleClass("clickClass", clickCount >= paginCount - 1)
    }
}

chkSize2 = () => {
    let x = $(document).width();

    pageLimit = (x <= 992) ? 5 : 10;
    paginIndex = 1;

    chkStatus()
}

$(() => {
    chkSize2()

    //$(window).on("resize", () => { chkSize2() })
    $(window).on("resize", chkSize2)

    //_accordion
    $(".btn_type").on("click", function () {
        $("#Cbox_type").val($(this).text())
        $("#Cbtn_submit").trigger("click")
    })

    $(".btn_author").on("click", function () {
        $("#Cbox_author").val($(this).text())
        $("#Cbtn_submit").trigger("click")
    })

    $(".btn_publisher").on("click", function () {
        $("#Cbox_publisher").val($(this).text())
        $("#Cbtn_submit").trigger("click")
    })

    $(".btn_lang").on("click", function () {
        $("#Cbox_lang").val($(this).text())
        $("#Cbtn_submit").trigger("click")
    })

    $(".btn_year").on("click", function () {
        let x = $(this).text().split("~")[0]
        let y = $(this).text().split("~")[1]

        $("#Cbox_year1").val(x)
        $("#Cbox_year2").val(y)
        $("#Cbtn_submit").trigger("click")
    })

    $("#Cbtn_reset").on("click", () => { $("#Cbtn_submit").trigger("click") })

    //_book
    $(".btn_book1").on("click", function () {
        let borrowStatus = $("#box_borrowStatus").val();
        let borrowCount = parseInt($("#box_borrowCount").val(), 10);

        $("#box_statusR").val("C")

        if ($("#id_user1").val() === "") {
            let loginC = $(".btn_guideLC").attr("href");

            alert("請先登入")

            window.location.href = loginC
        }
        else if (borrowStatus == "True") {
            alert("請先還書，尚有逾期書籍未歸還")
        }
        else if (borrowCount == 5) {
            alert("請先還書，借閱書籍不可超過5本")
        }
        else {
            let collectionId = $(this).closest(".col-3").find(".box_collection").val();
            let status = $(this).closest(".col-3").find(".btn_book1").text();

            if (status === "借閱") { alert(" 借閱成功，需在3天內前往取書\n(依通知為準，借閱相關規定請至訊息中心查看)") }
            else { alert(" 預約成功，可取書時將發送通知\n(依通知為準，借閱相關規定請至訊息中心查看)") }

            $("#id_collection1").val(collectionId)
            $("#btn_cReserveS").trigger("click")
        }
    })

    $(".btn_book2").on("click", function () {
        let boxBook = $(this).closest(".box_book");
        let bookTable = boxBook.find("table");
        let bookTitle = bookTable.find("h4").text();
        //let bookAuthor = bookTable.find("th:contains('作者')").next("td").text();
        //let bookTranslator = bookTable.find("th:contains('譯者')").next("td").text();
        //let bookPublisher = bookTable.find("th:contains('出版社')").next("td").text();
        //let bookLanguage = bookTable.find("th:contains('語言')").next("td").text();
        //let bookIsbn = bookTable.find("th:contains('ISBN')").next("td").text();

        $("#box_search").val(bookTitle)
        $("#btn_search1").trigger("click")
    })

    $(".pageClass").on("click", function () {
        const man = $("#box_man i");
        let x = $(this).data("bs-slide-to");
        let y = x % pageCount;

        if (x !== undefined) {
            man.css("color", "transparent");

            if (y >= 0 && y < man.length) { man.eq(y).css("color", "black"); }
        }

        window.scrollTo({ top: 0, behavior: "smooth" });
    })

    $("#btn_next").on("click", () => {
        if (clickCount < paginCount - 1) {
            paginIndex++;
            clickCount++;

            chkStatus()

            $(`#btn_${clickCount * pageLimit + 1}`).trigger("click")
        }
    })

    $("#btn_prev").on("click", () => {
        if (clickCount > 0) {
            paginIndex--;
            clickCount--;

            chkStatus()

            $(`#btn_${(clickCount + 1) * pageLimit}`).trigger("click")
        }
    })
})