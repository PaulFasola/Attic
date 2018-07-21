    function getRandomColor() {
        return "rgb(" + (Math.floor(Math.random() * 256)) + "," + (Math.floor(Math.random() * 256)) + "," + (Math.floor(Math.random() * 256)) + ")";
    }

    function changeBackgroundColor(e) {
        $(e).animate({
            backgroundColor: getRandomColor()
        }, 1300, function() {});
    }