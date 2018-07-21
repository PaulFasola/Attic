'use strict';

const electron = require('electron');
const BrowserWindow = electron.BrowserWindow;
module.exports = {

    /**
     * LoadUrlTo
     */

    "loadURLTo": function (window, dest, shouldClean) {
        if (shouldClean) {
            this.Cleanup();
        }
        try {
            window.loadURL(dest)
        } catch (e) {
            console.log('LoadUrl error, windows is null or unloaded before');
        }
    },

    /**
     * Cleanup : cleans the history, the storage and the application's cache
     */

    "Cleanup": function (window) {
        console.log("------ Clearing cache and storage operations ------");
        window.webContents.clearHistory();
        window.webContents.session.clearCache(function () {
            window.webContents.session.clearStorageData(function () {
                console.log("   -> Cache and storage data wiped successfully");
            });
        });
    },

    /**
     * CreateWindowInstance : creates an electron window
     */

    "CreateWindowInstance": function (show) {
        var instance = new BrowserWindow({
            width: 1000,
            height: 600,
            frames: false,
            show: show,
            resizable: false
        });
        instance.setMenu(null);
        return instance;
    }
};