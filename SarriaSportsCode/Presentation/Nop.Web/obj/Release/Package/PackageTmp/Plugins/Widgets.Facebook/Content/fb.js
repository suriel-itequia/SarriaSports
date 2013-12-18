var fb_c = "0";

$(function () {
    $("#fb").css("right", "0");
});

function Zdjecie() {
    if (fb_c == "0") {
        $("#fb").animate({ right: "-8" }, 1000);
        $("#facebox-like-box").animate({ right: "0" }, 1000);

        fb_c = "1"
    }
    else {
        $("#fb").animate({ right: "-300" }, 1000);
        $("#facebox-like-box").animate({ right: "-292" }, 1000);
        fb_c = "0"
    }
}


var Like = {

    addLike: function (urladd) {
        $.ajax({
            cache: false,
            url: urladd,
            type: 'post',
            error: this.ajaxFailure
        });
    },

    removeLike: function (urlremove) {
        $.ajax({
            cache: false,
            url: urlremove,
            type: 'post',
            error: this.ajaxFailure
        });
    },

    ajaxFailure: function () {
        alert('Failed to add like fan page. Please refresh the page and try one more time.');
    }

}