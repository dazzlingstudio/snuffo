//adds the resource to umbraco.resources module:
angular.module('umbraco.resources').factory('productTreePickerResource',
    function ($q, $http, umbRequestHelper) {
        //the factory object returned
        return {
            //this calls the ApiController 
            GetStores: function () {
                return umbRequestHelper.resourcePromise(
                    $http.get("backoffice/uvendiaplugin/CatalogApi/GetStores"),
                    "Failed to retrieve store");
            }            
        };
    }
);