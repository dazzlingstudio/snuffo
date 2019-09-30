
//var app = angular.module('app', ['umbraco.services']);

(function ($) {

    Uvendia.UmbracoClientManager = function () {
        function getRootInjector() {

            var parentAngular = window.parent.angular;
            return parentAngular.element(window.parent.document.getElementById("umbracoMainPageBody")).injector();
        }

        function getInjector() {

            //var parentAngular = window.parent.angular;
            return angular.element(window.parent.document.getElementById("uvendia-body")).injector();            
        }

        function getRootScope() {
            var parentAngular = window.parent.angular;
            return parentAngular.element(document.getElementById("umbracoMainPageBody")).scope();
        }           

        return {
            _isDirty: false,
            _isDebug: false,
            _mainTree: null,
            _appActions: null,
            _historyMgr: null,            
            getRootInjector: function () {
                return getRootInjector();
            },
            contentFrame: function (strLocation) {

                var injector = getRootInjector();

                var rootScope = injector.get("$rootScope");
                var angularHelper = injector.get("angularHelper");
                var navService = injector.get("navigationService");

                if (strLocation.startsWith("/")) {
                    strLocation = strLocation.trimStart("/");
                }
                strLocation = strLocation.replace(/\//g, '%2f'); // escape the slashes

                if (!strLocation || strLocation === "") {
                    //SD: NOTE: We used to return the content iframe object but now I'm not sure we should do that ?!

                    if (typeof top.right !== "undefined") {
                        return top.right;
                    }
                    else {
                        return top; //return the current window if the content frame doesn't exist in the current context
                    }
                }
                angularHelper.safeApply(rootScope, function () {
                    navService.loadLegacyIFrame(strLocation.toLowerCase());
                });
            },
            mainTree: function () {

                var injector = getRootInjector();
                var navService = injector.get("navigationService");
                var angularHelper = injector.get("angularHelper");
                var $rootScope = injector.get("$rootScope");

                //mimic the API of the legacy tree
                var tree = {

                    syncTree: function (path, forceReload) {
                        angularHelper.safeApply($rootScope, function () {
                            navService._syncPath(path, forceReload);
                        });
                    },
                    refreshTree: function (treeAlias) {
                        angularHelper.safeApply($rootScope, function () {
                            navService._setActiveTreeType(treeAlias, true);
                        });
                    }
                };

                return tree;
            },
            openModalWindow: function (url, title, size, onCloseCallback) {
                //need to create the modal on the top window if the top window has a client manager, if not, create it on the current window   
                var injector = getRootInjector();
                var editorService = injector.get("editorService");
                var rootScope = injector.get("$rootScope");
                
                editorService.open({
                    view: url,
                    size: size,
                    title: title
                });

                if (typeof rootScope.modal === "undefined") {
                    rootScope.modal = [];
                }
                rootScope.modal.push(onCloseCallback);
            },
            closeModalWindow: function (rVal) {                
                //get our angular navigation service
                var injector = getRootInjector();
                var editorService = injector.get("editorService");
                var rootScope = injector.get("$rootScope");
                
                // all legacy calls to closeModalWindow are expecting to just close the last opened one so we'll ensure
                // that this is still the case.
                if (typeof rootScope.modal !== "undefined" && rootScope.modal !== null && rootScope.modal.length > 0) {
                    
                    var lastModal = rootScope.modal.pop();

                    //if we've stored a callback on this modal call it before we close.
                    var self = this;
                    //get the compat callback from the modal element
                    var onCloseCallback = lastModal;
                    if (typeof onCloseCallback === "function") {
                        onCloseCallback.apply(self, [{ outVal: rVal }]);
                    }                    
                }    
                
                editorService.close();
            },
            showMessage: function (icon, header, message) {
                //get our angular navigation service
                var injector = getRootInjector();
                var notifyService = injector.get("notificationsService");

                switch (icon) {
                    case "save":
                        notifyService.success(header, message);
                        break;
                    case "success":
                        notifyService.success(header, message);
                        break;
                    case "warning":
                        notifyService.warning(header, message);
                        break;
                    case "error":
                        notifyService.error(header, message);
                        break;
                    default:
                        notifyService.info(header, message);
                }
            },
            setUmbracoPath: function () {
                // Umbraco 8 is still calling this function somewhere, without actually using it.
            }
        };
    };    
})(jQuery);

if (typeof UmbClientMgr === "undefined") {
    var UmbClientMgr = new Uvendia.UmbracoClientManager();
}
