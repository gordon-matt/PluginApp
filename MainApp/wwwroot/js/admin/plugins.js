'use strict'

var apiUrl = "/api/plugins";

var ViewModel = function () {
    var self = this;
    
    self.systemName = ko.observable(null);
    self.friendlyName = ko.observable(null);
    self.displayOrder = ko.observable(0);

    self.validator = false;

    self.init = function () {
        currentSection = $("#grid-section");
        
        self.validator = $("#form-section-form").validate({
            rules: {
                FriendlyName: { required: true, maxlength: 255 },
                DisplayOrder: { required: true, digits: true }
            }
        });

        $("#Grid").kendoGrid({
            dataSource: {
                transport: {
                    read: {
                        url: apiUrl + "/get",
                        type: "POST",
                        dataType: "json"
                    }
                },
                schema: {
                    data: 'Data',
                    total: 'Total',
                    model: {
                        fields: {
                            Group: { type: "string" },
                            FriendlyName: { type: "string" },
                            Installed: { type: "boolean" }
                        }
                    }
                },
                pageSize: 10,
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true,
                sort: { field: "Group", dir: "asc" }
            },
            dataBound: function (e) {
                var body = this.element.find("tbody")[0];
                if (body) {
                    ko.cleanNode(body);
                    ko.applyBindings(ko.dataFor(body), body);
                }
            },
            filterable: true,
            sortable: {
                allowUnsort: false
            },
            pageable: {
                refresh: true
            },
            scrollable: false,
            columns: [{
                field: "Group",
                title: "Group",
                filterable: true
            }, {
                field: "FriendlyName",
                title: "Plugin Info",
                template: '<b>#:FriendlyName#</b>' +
                    '<br />Version: #:Version#' +
                    '<br />Author: #:Author#' +
                    '<br />SystemName: #:SystemName#' +
                    '<br />DisplayOrder: #:DisplayOrder#' +
                    '<br />Installed: <i class="fa #=Installed ? \'fa-ok-circle fa-2x text-success\' : \'ffa-no-circle fa-2x text-danger\'#"></i>' +
                    '<br /><a data-bind="click: edit.bind($data,\'#=SystemName#\')" class="btn btn-default btn-sm">Edit</a>',
                filterable: false
            }, {
                field: "Installed",
                title: " ",
                template:
                    '# if(Installed) {# <a data-bind="click: uninstall.bind($data,\'#=SystemName#\')" class="btn btn-default btn-sm">Uninstall</a> #} ' +
                    'else {# <a data-bind="click: install.bind($data,\'#=SystemName#\')" class="btn btn-success btn-sm">Install</a> #} #',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 130
            }]
        });
    };

    self.edit = function (systemName) {
        systemName = self.replaceAll(systemName, ".", "-");
        
        $.ajax({
            url: apiUrl + "/" + systemName,
            type: "GET",
            dataType: "json",
            async: false
        })
        .done(function (json) {
            self.systemName(systemName);
            self.friendlyName(json.FriendlyName);
            self.displayOrder(json.DisplayOrder);
            
            self.validator.resetForm();
            switchSection($("#form-section"));
            $("#form-section-legend").html("Edit");
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify("There was a problem retrieving the record.", "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };

    self.save = function () {
        if (!$("#form-section-form").valid()) {
            return false;
        }

        var record = {
            FriendlyName: self.friendlyName(),
            DisplayOrder: self.displayOrder()
        };

        $.ajax({
            url: apiUrl + "('" + self.systemName() + "')",
            type: "PUT",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(record),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            $('#Grid').data('kendoGrid').dataSource.read();
            $('#Grid').data('kendoGrid').refresh();

            switchSection($("#grid-section"));

            $.notify("Succesfully updated record.", "success");
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify("Error when trying to update record.", "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };

    self.cancel = function () {
        switchSection($("#grid-section"));
    };

    self.install = function (systemName) {
        systemName = self.replaceAll(systemName, ".", "-");

        $.ajax({
            url: "/admin/plugins/install/" + systemName,
            type: "POST"
        })
        .done(function (json) {
            if (json.Success) {
                $.notify(json.Message, "success");
            }
            else {
                $.notify(json.Message, "error");
            }

            setTimeout(function () {
                window.location.reload();
            }, 1000);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify("Error when trying to install plugin", "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    }

    self.uninstall = function (systemName) {
        systemName = self.replaceAll(systemName, ".", "-");

        $.ajax({
            url: "/admin/plugins/uninstall/" + systemName,
            type: "POST"
        })
        .done(function (json) {
            if (json.Success) {
                $.notify(json.Message, "success");
            }
            else {
                $.notify(json.Message, "error");
            }

            setTimeout(function () {
                window.location.reload();
            }, 1000);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify("Error when trying to uninstall plugin", "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    }

    self.replaceAll = function (string, find, replace) {
        return string.replace(new RegExp(self.escapeRegExp(find), 'g'), replace);
    };

    self.escapeRegExp = function (string) {
        return string.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
    };
}