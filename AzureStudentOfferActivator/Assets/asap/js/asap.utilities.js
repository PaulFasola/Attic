$(document).ready(function() {
    var _email = "";
    var _dsStatus = false;
    var _manual = false;
    var lang = $.lang("companion");
    var lg = lang.getRessourceGroup("js-index");

    var ENDPOINT_URL = "http://azure-contest.cloudapp.net/src/classes/xhrRoutine.php?fr-FR";

    $("#code_checkin").css('display', "none");
    $("#email_checkin").css('display', "none");
    $("#back").css("display", "none");

    $("#ds_ok").click(function() {
        _dsStatus = true;
        $("#email_checkin").css('display', "initial");
        $("#possibilities").css('display', "none");
        $("#ds_status").text(lg.status2);
        $("#back").css("display", "initial");
        _manual = true;
    });

    $("#ds_nonok").click(function() {
        $("#email_checkin").css('display', "initial");
        $("#possibilities").css('display', "none");
        $("#ds_status").text(lg.status1);
        $("#back").css("display", "initial");
    });

    $('#email_checkin').on('submit', function(e) {
        e.preventDefault();
        var $this = $(this);
        var mail = $('#email').val();
        var re = /^[-a-z0-9~!$%^&*_=+}{\'?]+(\.[-a-z0-9~!$%^&*_=+}{\'?]+)*@([a-z0-9_][-a-z0-9_]*(\.[-a-z0-9_]+)*\.(aero|arpa|biz|com|coop|edu|gov|info|int|mil|museum|name|net|org|pro|travel|mobi|[a-z][a-z])|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,5})?$/i;

        $("input").prop('disabled', true);
        $(".loading").css("display", "initial")

        _email = mail;

        if (mail == "" || !re.test(mail)) {
            notify("L'adresse email entrée n'est pas valide.", "error");
            $("input").prop('disabled', false);
            $(".loading").css("display", "none");
            return;
        }
        $.ajax({
            url: ENDPOINT_URL,
            type: "POST",
            data: {"email": $('#email').val() },
            success: function(html) {
                $(".loading").css("display", "none")
                switch (html) {
                    case "EMPTY":
                        notify(lg.email_error, "error");
                        break;
                    case "NO_MORE_TOKEN":
                        notify("Désolé, pénurie de jeton pour le moment :( N'hésitez pas à revenir de temps en temps", "warning");
                        break;
                    case "FAILURE":
                        notify("Désolé, une erreur interne est survenue, merci de réitérer", "error");
                        break;
                    default:
                        if (html.indexOf('#') !== -1 || html == "ALREADY_SET") {
                            if (html !== "ALREADY_SET")
                                notify("Un email vient d'être envoyé à l'adresse indiquée (vérifiez vos spam !).", "success");

                            $("#email_checkin").css('display', "none");
                            $("#code_checkin").css('display', "initial");
                            $("#code_cap").append("Merci d'entrer le code à 6 chiffres reçu par email (à l'adresse '" + mail + "' )");

                        } else {
                            notify(html, "info");
                        }
                        break;
                }
                $("input").prop('disabled', false);
            }
        });
    });

    $("#back").click(function(e) {
        e.preventDefault();
        $("#back").css("display", "none");
        $("code_checkin").css("display", "none");
        $("#email_checkin").css("display", "none");
        $("#possibilities").css("display", "initial");
        $("#ds_status").text("");
    });

    $('#code_checkin').on('submit', function(e) {
        e.preventDefault();
        var $this = $(this);
        var code = $('#code').val();
        $("input").prop('disabled', true);
        $(".loading").css("display", "initial")

        if (code == "" || code.length != 6) {
            notify("Code incorrect.");
            $("input").prop('disabled', false);
            $(".loading").css("display", "none")
            return;
        }
        $.ajax({
            url: ENDPOINT_URL + (_manual ? "&manual" : ""),
            type: "POST",
            data: {
                email: _email,
                code: code
            },
            success: function(html) {
                $(".loading").css("display", "none")
                switch (html) {
                    case "ACCESS_DENIED":
                        alert(html);
                        notify("Code incorrect.");
                        break;
                    default:
                        if (!_manual)
                            notify(lg.token_retrieved, "success");
                        else {
                            html = "manuel";
                        }
                        setTimeout(function() {
                            window.location.href = $(location).attr('href') + "?retrieved_token=" + html;
                        }, 3000);
                        break;
                }
                $("input").prop('disabled', false);
            }
        });
    });

    function notify(text, type, t) {
        t = typeof t !== 'undefined' ? t : 5000;
        noty({
            text: text,
            timeout: t,
            type: type,
            animation: {
                position: 'top right',
                elementPosition: 'bottom left',
                globalPosition: 'top right',
                open: 'animated fadeIn',
                close: 'animated fadeOut',
                easing: 'swing',
                speed: 500
            }
        });
    }
});