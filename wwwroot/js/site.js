let clickTimes = 0;

chkSize1 = () => {
    let x = $(document).width();

    if (x <= 768) { $(".box_info").addClass("d-none"); }
    else { $(".box_info").removeClass("d-none"); }
}

$(() => {
    chkSize1()

    //$(window).on("resize", () => { chkSize1() })
    $(window).on("resize", chkSize1)

    //header
    $(".link").on("click", () => { if ($(".navbar-toggler").is(":visible")) { $(".navbar-toggler").trigger("click") } })

    //footer
    $("#btn_manage").on("click", () => {
        clickTimes += 1;

        if (clickTimes % 3 == 0) {
            const manageUrl = $(".btn_guideM").attr("href");

            if (manageUrl) { window.location.href = manageUrl; }
        }
    })
})
