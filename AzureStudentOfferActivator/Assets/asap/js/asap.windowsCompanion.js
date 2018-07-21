 ;
 (function($) {
     $.timer = function(func, time, autostart) {
         this.set = function(func, time, autostart) {
             this.init = true;
             if (typeof func == 'object') {
                 var paramList = ['autostart', 'time'];
                 for (var arg in paramList) {
                     if (func[paramList[arg]] != undefined) {
                         eval(paramList[arg] + " = func[paramList[arg]]");
                     }
                 };
                 func = func.action;
             }
             if (typeof func == 'function') {
                 this.action = func;
             }
             if (!isNaN(time)) {
                 this.intervalTime = time;
             }
             if (autostart && !this.isActive) {
                 this.isActive = true;
                 this.setTimer();
             }
             return this;
         };
         this.once = function(time) {
             var timer = this;
             if (isNaN(time)) {
                 time = 0;
             }
             window.setTimeout(function() {
                 timer.action();
             }, time);
             return this;
         };
         this.play = function(reset) {
             if (!this.isActive) {
                 if (reset) {
                     this.setTimer();
                 } else {
                     this.setTimer(this.remaining);
                 }
                 this.isActive = true;
             }
             return this;
         };
         this.pause = function() {
             if (this.isActive) {
                 this.isActive = false;
                 this.remaining -= new Date() - this.last;
                 this.clearTimer();
             }
             return this;
         };
         this.stop = function() {
             this.isActive = false;
             this.remaining = this.intervalTime;
             this.clearTimer();
             return this;
         };
         this.toggle = function(reset) {
             if (this.isActive) {
                 this.pause();
             } else if (reset) {
                 this.play(true);
             } else {
                 this.play();
             }
             return this;
         };
         this.reset = function() {
             this.isActive = false;
             this.play(true);
             return this;
         };
         this.clearTimer = function() {
             window.clearTimeout(this.timeoutObject);
         };
         this.setTimer = function(time) {
             var timer = this;
             if (typeof this.action != 'function') {
                 return;
             }
             if (isNaN(time)) {
                 time = this.intervalTime;
             }
             this.remaining = time;
             this.last = new Date();
             this.clearTimer();
             this.timeoutObject = window.setTimeout(function() {
                 timer.go();
             }, time);
         };
         this.go = function() {
             if (this.isActive) {
                 this.action();
                 this.setTimer();
             }
         };

         if (this.init) {
             return new $.timer(func, time, autostart);
         } else {
             this.set(func, time, autostart);
             return this;
         }
     };
 })(jQuery);

 function WindowsCompanion(options) {
     this.captions = options.captions;
     this.category = options.category;
     this.speed = 3000; // Default
     this.row = 0;
     this.firstSlide = true;
     this.timeout_handles = [];

     var $this = this;

     var nextTimer = $.timer(function() {
         $this.NextCaption();
     });
     nextTimer.set({
         time: 3000,
         autostart: false
     });

     var fadeTimer = $.timer(function() {
         $this.Fade();
     });
     fadeTimer.set({
         time: 3000,
         autostart: false
     });

     this.Render = function() {
         if (this.captions === null)
             throw "Error, caption array is not defined.";
         this.NextCaption();
     };

     this.NextCaption = function() {
         var line = $this.captions[$this.category][$this.row];
         $("#caption").removeClass().addClass(line.effect);
         $("#caption").text(line.text);
         nextTimer.stop();
         fadeTimer.play();
         $this.row++;
     }

     this.Fade = function() {
         $("#caption").removeClass().addClass("animated fadeOut");
         fadeTimer.stop();
         nextTimer.play();
     }
 }

 function getUrlVars() {
     var vars = [],
         hash;
     var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
     for (var i = 0; i < hashes.length; i++) {
         hash = hashes[i].split('=');
         vars.push(hash[0]);
         vars[hash[0]] = hash[1];
     }
     return vars;
 }

 var companion = new WindowsCompanion({
     captions: table,
     category: getUrlVars()["scenario"]
 });

 companion.Render();