;
(function($) {
    $.lang = function(defaultPath) {
        this.defaultPath = defaultPath;
        this.init = false;
        this.res = null;

        var data = null;
        var navLang = (navigator.language || navigator.userLanguage) || "en-US"; 

        this.getRessourceGroup = function(moduleName) {
            if (!this.init) {
                throw ("Globalization not enabled, it must be initialized before.");
            }
            return this.res.modules[this.defaultPath][moduleName];
        };

        if (!this.init) {
            try {
                var fs = require('fs');
                data = fs.readFileSync("Assets/asap/js/ressources/lang." + navLang + ".js", "utf8").toString();
            } catch (e) {
                console.info("Navigator lang not found or handled, selecting 'en-US' by default.");
                // Default
                data = fs.readFileSync("Assets/asap/js/ressources/lang." + navLang + ".js");
                this.res = data;
            }

            try {
                this.res = $.parseJSON(data);
            } catch (e) {
                console.error("Parsing lang data failed : is the file '" + navLang + "' is missing or corrupted ? More details : " + e);
            }
            this.init = true;
            return this;
        }
    };
})(jQuery);