'use strict';

const electron = require('electron');
const app = electron.app;
const BrowserWindow = electron.BrowserWindow;

const utils = require("./modules/utilities");

// @bool - If the user choose between activating DreamSpark nor Azure
var _manualActivation = false;

// @bool - If it's the first activation attempt of the current instance
var _isFirstAttempt = true;

// @bool - If the fix concerning the Azure's cache is applied or not
var _azureFixApplied = false;

// @bool - If the user is logged into DreamSpark
var _logged = false;

// @bool - If Azure Portal was visited before
var _azureAlreadyEntered = false;

// @string - The value is retrieved from the database if the user requested it
var _token = null;

// @bool - Has the fix been successfully applied?
var _fixApplied = false;

// Windows declaration
let mainWindow;
let loginWindow;
let loadingWindows;

// URLS
var DREAMSPARK_URLS = {
    HOME: "https://www.dreamspark.com/student/default.aspx?wa=wsignin1.0&lc=1036",
    PRODUCT: "https://www.dreamspark.com/Product/Product.aspx?productid=99",
    VERIFICATION: "https://www.dreamspark.com/Account/MyAccount.aspx?GetVerified=true"
}

var AZURE_URLS = {
    MAIN: "http://portal.azure.com"
}

app.on('ready', function() {
    loadingWindows = utils.CreateWindowInstance(false);
    mainWindow = utils.CreateWindowInstance();

    mainWindow.webContents.on('dom-ready', function() {
        var url = mainWindow.webContents.getURL();

        if (url.indexOf("retrieved_token") > -1) {
            if (url.indexOf("manuel") > -1) {
                console.log("Manual activation requested. Going directly to Azure.");
                _manualActivation = true;
            } else {
                var token = url.substr(url.length - 29);
                _token = token.split("-");
                console.log("Token " + token + " stored. Going to DS activation process.");
            }

            mainWindow.hide();
            utils.loadURLTo(loadingWindows, `file://${__dirname}/entrance.html?scenario=logintods`);
            loadingWindows.show();
            loadLoginScreen();
            setTimeout(showLoginWindows, 5000);
        }
    });

    mainWindow.on('closed', function() {
        mainWindow = null;
    });

    loadingWindows.on('closed', function() {
        loadingWindows = null;
    });

    utils.loadURLTo(mainWindow, `file://${__dirname}/index.html`);
});

app.on('window-all-closed', function() {
    app.quit();
});

function checkTimeout() {
    var url = loginWindow.webContents.getURL();
    if (url.indexOf("GetVerified") > -1) {
        if (_isFirstAttempt) {
            _isFirstAttempt = false;
            utils.loadURLTo(loginWindow, DREAMSPARK_URLS.VERIFICATION);
            return;
        }
        console.log("FAILURE");
        utils.loadURLTo(loginWindow, `file://${__dirname}/failure.html?token=` + _token);
        closeLoadingWindow();
        showLoginWindows();
        return;
    } else {
        utils.loadURLTo(loginWindow, DREAMSPARK_URLS.PRODUCT);
    }
}

function loadLoginScreen() {
    loginWindow = utils.CreateWindowInstance(false);
    utils.Cleanup(loginWindow);
    utils.loadURLTo(loginWindow, DREAMSPARK_URLS.HOME);

    loginWindow.webContents.on('dom-ready', function() {
        var url = loginWindow.webContents.getURL();
        console.log("URL ======> " + url);
        if (url === DREAMSPARK_URLS.HOME) {
            if (!_logged) {
                loginWindow.webContents.executeJavaScript("__doPostBack('ctl00$ctl00$ContentPlaceHolder1$LinkButton_SignIn','');");
                _logged = true;
                return;
            }
            if (_fixApplied || _manualActivation) {
                utils.loadURLTo(loginWindow, DREAMSPARK_URLS.PRODUCT);
                return;
            }
            utils.loadURLTo(loginWindow, DREAMSPARK_URLS.VERIFICATION);
        }

        if (url.indexOf("getverified") > -1 || url.indexOf("GetVerified") > -1) {
            if (_isFirstAttempt) {
                utils.loadURLTo(loadingWindows, `file://${__dirname}/entrance.html?scenario=activatingds`);
                loginWindow.hide();
                loadingWindows.show();
                if (_token == null) {
                    utils.loadURLTo(loginWindow, `file://${__dirname}/failure.html?errid=1`);
                    return;
                }
                console.log("Done. Injecting token...");

                // Filing all the textblocks with the token's parts (without '-')
                loginWindow.webContents.executeJavaScript('document.getElementById("ctl00_ctl00_ContentPlaceHolder1_StudentBody_userVerificationControl_TextBox_VC1").value = "' + _token[0] + '";document.getElementById("ctl00_ctl00_ContentPlaceHolder1_StudentBody_userVerificationControl_TextBox_VC2").value = "' + _token[1] + '";document.getElementById("ctl00_ctl00_ContentPlaceHolder1_StudentBody_userVerificationControl_TextBox_VC3").value = "' + _token[2] + '";document.getElementById("ctl00_ctl00_ContentPlaceHolder1_StudentBody_userVerificationControl_TextBox_VC4").value = "' + _token[3] + '";document.getElementById("ctl00_ctl00_ContentPlaceHolder1_StudentBody_userVerificationControl_TextBox_VC5").value = "' + _token[4] + '";');
                console.log("Done. Calling verification process.");

                // Performs a click on the "Validate button"
                loginWindow.webContents.executeJavaScript("document.getElementById('ctl00_ctl00_ContentPlaceHolder1_StudentBody_userVerificationControl_LinkButton_VerificationByCode').click();");
                setTimeout(checkTimeout, 20000);
            } else {
                /* At this state, we dont know the activation status.
                   To be certain that the process is completed (cause' of dynamic JS)
                   We have to wait... and redirect to a page that will tell us
                   if this last is a success, or not. */
                console.log("Dreamspark activation undetermined for now. Injecting javascript to determine..")
                loginWindow.webContents.executeJavaScript('if(document.getElementById("ctl00_ctl00_ContentPlaceHolder1_StudentBody_Label_ExpirationDateText") != undefined)window.location.href="https://www.dreamspark.com/Student/Software-Catalog.aspx";');
                setTimeout(checkTimeout, 20000);
            }
        }

        if (url.indexOf("software-catalog.aspx") > -1) {
            console.log("Dreamspark ACTIVATED successfully. Continuing!");
            utils.loadURLTo(loginWindow, DREAMSPARK_URLS.PRODUCT);
        }

        if (url.indexOf("codeverificationsucceeded=true") > -1) {
            console.log("Dreamspark is now activated for this account. Executing the Azure fix.")
            utils.Cleanup(loginWindow);
            _fixApplied = true;
            utils.loadURLTo(loginWindow, DREAMSPARK_URLS.HOME);
        }

        // Microsoft main login page that redirect to DS once the user logged
        if (url.indexOf("login.srf?") > -1) {
            console.log("Microsoft login page : applying actions");

            var jsInstruction = 'document.getElementById("idA_PWD_ForgotPassword").outerHTML="";';
                jsInstruction += 'document.getElementById("idA_PWD_SwitchToOTC").outerHTML="";';
                jsInstruction += 'document.getElementById("learnMoreLink").outerHTML="";';
            loginWindow.webContents.executeJavaScript(jsInstruction);

            console.log("Microsoft login page : JS injection complete all exit points are disabled.");

            loadingWindows.hide();
            loginWindow.show();
        }

        if (url.indexOf("productid=99") > -1) {
            loginWindow.webContents.executeJavaScript("document.getElementById('LinkButton_RegisterAzure').click();");
            setTimeout(closeLoadingWindow, 5000);
            setTimeout(showLoginWindows, 6000);
        }

        if (url.indexOf("offer=ms-azr-0144P") > -1) {
            console.log("Entering Azure portal...");
            utils.loadURLTo(loadingWindows, `file://${__dirname}/entrance.html?scenario=enteringportal`);
            loadingWindows.show();
            loginWindow.hide();

            setTimeout(function() {
                closeLoadingWindow();
                showLoginWindows();

            var jsInstruction = '$("#offer-info-container").text("");';
                jsInstruction += '$(".email-details").remove();';
                jsInstruction += '$("label[for=\'CustomerInfo_WorkPhone\']").text("TÉLÉPHONE");';
                jsInstruction += '$(".locale-selector").remove();';
                jsInstruction += '$(".footer-bottom").remove();';
                jsInstruction += '$(".offer-name-signup-azure").text("Dreamspark and Azure Student Activator");';
                jsInstruction += '$(".vatId").css("display", "none");';
            loginWindow.webContents.executeJavaScript(jsInstruction);

            console.log("Azure signup page : JS injection complete all exit points are disabled.");
            _azureAlreadyEntered = true;
            }, 15000);
        }

        if (url.indexOf("portal.azure") > -1 && _azureAlreadyEntered) {
            require("shell").openExternal(AZURE.MAIN);
            require('app').quit();
        }
    });

    loginWindow.on('closed', function() {
        loginWindow = null;
    });
}

function closeLoadingWindow() {
    loadingWindows.show(false);
}

function showLoginWindows() {
    loginWindow.show(true);
}