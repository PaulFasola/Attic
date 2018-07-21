$(window).load(function() {
    centerContent();
});

$(window).resize(function() {
    centerContent();
});

function centerContent() {
    var container = $('#container');
    var content = $('#caption');
    content.css("left", (container.width() - content.width()) / 2);
    content.css("top", (container.height() - content.height()) / 2);
}

var lg = $.lang("companion");
var init = lg.getRessourceGroup("initialization");
var lds = lg.getRessourceGroup("logintods");
var ads = lg.getRessourceGroup("activatingDS");
var gta = lg.getRessourceGroup("goingtoazure");
var epl = lg.getRessourceGroup("enteringportal");

var table = {

    "logintods": [{
        text: lds.s01,
        effect: "animated bounceInRight"
    }, {
        text: lds.s02,
        effect: "animated fadeIn"
    }],
    "activatingds": [{
        text: ads.s01,
        effect: "animated bounceInLeft"
    }, {
        text: ads.s02,
        effect: "animated fadeIn"
    }, {
        text: ads.s03,
        effect: "animated fadeIn"
    }, {
        text: ads.s04,
        effect: "animated fadeIn"
    }, {
        text: ads.s05,
        effect: "animated fadeIn"
    }, {
        text: ads.s06,
        effect: "animated fadeIn"
    }, {
        text: ads.s07,
        effect: "animated fadeIn"
    }],
    "goingtoazure": [{
        text: gta.s01,
        effect: "animated bounceInLeft"
    }, {
        text: gta.s02,
        effect: "animated fadeIn"
    }, {
        text: gta.s03,
        effect: "animated fadeIn"
    }, {
        text: gta.s04,
        effect: "animated fadeIn"
    }],
    "enteringportal": [{
        text: epl.s01,
        effect: "animated bounceInLeft"
    }, {
        text: epl.s02,
        effect: "animated fadeIn"
    }, {
        text: epl.s03,
        effect: "animated fadeIn"
    }, {
        text: epl.s04,
        effect: "animated fadeIn"
    }]
}