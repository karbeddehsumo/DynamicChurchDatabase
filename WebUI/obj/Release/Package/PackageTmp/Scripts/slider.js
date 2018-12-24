/// <reference path="jquery-1.7.1.min.js" />

sliderInt = 1;
sliderNext = 2;
$(document).ready(function () {

    function Slider() {

   //     $("#slider #1").show(500);
        //    $("#slider #1").delay(700);
        $("#slider #1").show(300);
        $("#slider #1").delay(300);

        var sc = $("#slider img").size();
        var count = 2;

        setInterval(function () {
          //  $("#slider #" + count).fadeIn(1000);
            // $("#slider #" + count).delay(4000).fadeOut(1000);
            $("#slider #" + count).fadeIn(1000);
            $("#slider #" + count).delay(4000).fadeOut(1000);

            if (count == sc) {
                count = 1;
            }
            else {
                count = count + 1;
            }
        }, 3500);
    }
    $("#slider > img#1").fadeIn(300);
    Slider();
});

function prev() {
    newSlide = sliderInt - 1;
    showSlide(newSlide);
}


function next() {
    newSlide = sliderInt + 1;
    showSlide(newSlide);
}

function stopLoop() {
    window.clearInterval();
}


function showSlide(id) {
    stopLoop();
    if (id > count) {
        id = 1;
    } else if (id < 1) {
        id = count; 
    }

    $("#slider > img").fadeOut(300);
    $("#slider > img#" + id).fadeIn(300);

    sliderInt = id;
    sliderNext = id + 1;

    Slider();
}


$("#slider > img").hover(
    function () {
        stopLoop();
    },

    function () {
        slider();
    }
);


/*
$(document).ready(function () {

    function Slider() {

        $("#slider #1").show(500);
        $("#slider #1").delay(2000).slideUp(1500);



        var sc = $("#slider img").size();
        var count = 2;

        setInterval(function () {
            $("#slider #" + count).fadeIn(1000);
            $("#slider #" + count).delay(4000).fadeOut(1000);

            if (count == sc) {
                count = 1;
            }
            else {
                count = count + 1;
            }
        }, 6500);
    }
    $("#slider > img#1").fadeIn(300);
    Slider();
});

*/