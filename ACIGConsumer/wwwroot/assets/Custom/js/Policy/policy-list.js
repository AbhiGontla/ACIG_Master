"use strict";
// Class definition

var KTDatatableRemoteAjaxDemo = function () {
    // Private functions

    // basic demo
    var demo = function () {

        var datatable = $('#kt_datatable').KTDatatable({
            // datasource definition
            data: {
                type: 'remote',
                source: {
                    read: {
                        url: '/Policy/PolicyList',
                        // sample custom headers
                        // headers: {'x-my-custom-header': 'some value', 'x-test-header': 'the value'},
                        map: function (raw) {
                            // sample data mapping
                            var dataSet = raw;
                            if (typeof raw.data !== 'undefined') {
                                dataSet = raw.data;
                            }
                            return dataSet;
                        },
                    },
                },
                pageSize: 10,
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true,
            },

            // layout definition
            layout: {
                scroll: false,
                footer: false,
            },

            // column sorting
            sortable: true,

            pagination: true,

            search: {
                input: $('#policySearch'),
                key: 'policySearch'
            },

            // columns definition
            columns: [{
                field: 'policyNumber',
                title: 'PolicyNumber',
                sortable: 'asc',
            }, {
                    field: 'policyFromDate',
                    title: 'PolicyFromDate',
                },
                {
                    field: 'policyToDate',
                    title: 'To Date'
                },
                {
                    field: 'policyType',
                    title: 'Type'
                },
                {
                    field: 'clientType',
                    title: 'Client Type'
                },
                {
                    field: 'clientName',
                    title: 'Client Name'
                }
            ],

        });

        $('#kt_datatable_search_status').on('change', function () {
            datatable.search($(this).val().toLowerCase(), 'Status');
        });

        $('#kt_datatable_search_type').on('change', function () {
            datatable.search($(this).val().toLowerCase(), 'Type');
        });

        $('#kt_datatable_search_status, #kt_datatable_search_type').selectpicker();
    };

    return {
        // public functions
        init: function () {
            demo();
        },
    };
}();

jQuery(document).ready(function () {
    KTDatatableRemoteAjaxDemo.init();
});
