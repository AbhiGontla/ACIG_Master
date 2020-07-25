"use strict";
// Class definition

var KTDatatableClaims = function () {
    // Private functions

    // basic demo
    var demo = function () {

        var datatable = $('#kt_datatable').KTDatatable({
            // datasource definition
            data: {
                type: 'remote',
                source: {
                    read: {
                        url: '/Claims/ClaimList',
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
                input: $('#claimsSearch'),
                key: 'claimsSearch'
            },

            // columns definition
            columns: [{
                field: 'nationalID',
                title: 'NationalId',
                sortable: 'asc',
            }, {
                    field: 'insPolicyNo',
                title: 'InsPolicyNo',
            },
            {
                field: 'cL_STATUS',
                title: 'POLICY SEQ'
            },
            {
                field: 'cL_PROVNAME',
                title: 'PROVIDER NAME'
            },
            {
                field: 'nETPAYABLE_OR',
                title: 'NETPAYABLE OR'
            },
            {
                field: 'nETPAYABLE_LL',
                title: 'NETPAYABLE LL'
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
"use strict";
// Class definition

var KTDatatableReimbursment = function () {
    // Private functions

    // basic demo
    var demo = function () {

        var datatable = $('#kt_datatable').KTDatatable({
            // datasource definition
            data: {
                type: 'remote',
                source: {
                    read: {
                        url: '/Claims/ReimbursmentList',
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
                input: $('#claimsSearch'),
                key: 'claimsSearch'
            },

            // columns definition
            columns: [{
                field: 'requestNumber',
                title: 'Request Number',
                sortable: 'asc',
            }, {
                    field: 'policyNumber',
                    title: 'PolicyNumber',
            },
            {
                field: 'holderName',
                title: 'HolderName'
            },
            {
                field: 'memberName',
                title: 'Member NAME'
            },
            {
                field: 'claimTypeName',
                title: 'ClaimTypeName'
            },
            {
                field: 'expectedAmount',
                title: 'ExpectedAmount'
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

