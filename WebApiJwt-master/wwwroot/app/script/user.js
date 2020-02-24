﻿$(document).ready(function () {
    var selectedCompany = {};
    var selectedUser;
    var userList;
    var companyList;
    var mode = 0; //0:new, 1:edit
    var template = "<div class='items' data-id='{0}'><div class=header1>{1}</div><div class=title><div><img src='../images/user2.png'></div>{2} <br />{3}</div></div>";

   //카테고리 취득
    function getCompanies() {
        getService(companyEP, function (data) {
            var items = "";
            var companies = "<div class='menu-sub-item' data-value='0'>전체</div>";
            companyList = data;
            $(data).each(function (i, n) {
                items = items + "<li class='cItem' id='" + n.id + "'><a>" + n.companyName + "</a></li>";
                companies = companies + "<div class='menu-sub-item'  data-value='" + n.code + "'>" + n.companyName + "</div>";
            });

            $("#category").append(items);
            $("#companies").append(companies);
        });
    }


    //모든 사용자취득하기
    function getData(url, callback) {
        getService(userEP, function (data) {
            userList = data;
            bindUsers(data);
        });
    }

    function bindUsers(users) {
        $("#kpiContainer").html("");
        var usersT = "";
        $(users).each(function (i, n) {
            usersT += template.format([n.id.toString(), n.companyName, n.name, n.userID]);
        });
        if (usersT === '')
            $("#kpiContainer").append("<div>표시할 사용자가 없습니다.</div>");
        else
            $("#kpiContainer").append(usersT);
    }

    //저장하기
    function save(e) {

        //if ($('#myForm').validator('validate').has('.has-error').length === 0) {
        //    var recent = $.grep(userList, function (user) {
        //        return user.userID === $("#userID").val();
        //    });

        //    if (typeof selectedCompany.id === 'undefined') {
        //        $(".btn-dropdown").addClass("error");
        //        $(".valid").show();
        //        e.preventDefault();
        //    } else {
        //        $(".btn-dropdown").removeClass("error");
        //        $(".valid").hide();
        //    }
        //    if (recent.length > 0) {
        //        alert("이미 등록된 사용자입니다.");
        //        e.preventDefault();
        //        return;
        //    }   

        //    var data = {
        //        name: $("#name").val(),
        //        userID: $("#userID").val(),
        //        password: $("#pwd").val(),
        //        companyID: selectedCompany.id
        //    };

        //    saveService(registerEP, JSON.stringify(data), function (result) {
        //        if (result) {
        //            $('.modal').modal('toggle');
        //            getData();

        //        }
        //    });
        //}

        if (validate(e)) {
            var isManager = $("#isManager").prop("checked");

            var data = {
                name: $("#name").val(),
                userID: $("#userID").val(),
                password: $("#pwd").val(),
                companyID: selectedCompany.id,
                userRole: (isManager) ? 1 : 2
            };

         saveService(registerEP, JSON.stringify(data), function (result) {
                if (result) {
                    $('.modal').modal('toggle');
                    getData();

                }
            });
        }
        
    }

    //수정하기
    function update(e) {
        if (validate(e)) {
            var isManager = $("#isManager").prop("checked");

            var data = {
                id: selectedUser.id,
                name: $("#name").val(),
                userID: $("#userID").val(),
                password: $("#pwd").val(),
                companyID: selectedCompany.id,
                userRole: (isManager) ? 1 : 2

            };

            // var data = "?id=" + selectedKPI.id + "&title=" + $("#title").val() + "&url=" + $("#url").val() + "&category=" + selectedCategory;
            updateService(userEP, JSON.stringify(data), function (result) {
                if (result) {
                    $('.modal').modal('toggle');
                    getData();
                    reset();
                }
            });
        }
    }

    //Validation처리
    function validate(e) {
        var valid = true;
        //에러가 없으면,
        if ($('#myForm').validator('validate').has('.has-error').length === 0) {

            //회사지정안됨.
            if (typeof selectedCompany.id === 'undefined') {
                $(".btn-dropdown").addClass("error");
                $(".valid").show();
                e.preventDefault();
                valid = false;
            } else {
                $(".btn-dropdown").removeClass("error");
                $(".valid").hide();
            }
            if (mode === 0) {
                var recent = $.grep(userList, function (user) {
                    return user.userID === $("#userID").val();
                });

                if (recent.length > 0) {
                    alert("이미 등록된 사용자입니다.");
                    e.preventDefault();
                    valid = false;

                }
            }
            return valid;
        }
        return false;
    }

    //삭제하기
    function deleteKPI()
    {
        //20200109 김태규 수정 배포
        //deleteService(userEP, "?id=" + selectedUser.id, function (result) {
        deleteService(userEP, "?id=" + selectedUser.id + "&userid=" + selectedUser.userID, function (result) {
            if (result) {

                var target = $('div[data-id="' + selectedUser.id + '"]');
                target.remove();
                selectedKPI = null;
				//20200109 김태규 수정 배포
				mode = 1;
                getData();

            }
        });
        //2019-12-12 김태규 작성 배포
        //$.ajax({
        //    type: "DELETE",
        //    url: apiService + "account/DeleteUser?email=" + selectedUser.id,
        //    success: function (result) {

        //    }, error: function (result) {
        //    }
        //});
      
    }

    function getUserByID(id) {
        var result = $.grep(userList, function (kpi) {
            return kpi.id === id;
        });

        if (result.length > 0)
            return result[0];
        else
            alert("해당되는 KPI가 없습니다");
    }
    function getCompanyByID(id) {
        var result = $.grep(companyList, function (kpi) {
            return kpi.id === id;
        });

        if (result.length > 0)
            return result[0];
        else
            alert("해당되는 KPI가 없습니다");
    }

    function bindKPI(target) {
        reset();
        $('.modal').modal('toggle');
        var id = $(target).data("id");
        selectedUser = getUserByID(id);
        selectedCompany = getCompanyByID(selectedUser.companyID);
        $("#name").val(selectedUser.name).attr("disabled", "disabled");
        $("#userID").val(selectedUser.userID).attr("disabled", "disabled");
        $("#selectedCompany").html(selectedCompany.companyName);
        $("#pwd").val("******");
        mode = 1;

        $("#saveButton").text("수정");
    }

    function reset() {

        $("#name").val("");
        $("#userID").val("");
        //20200109 김태규 수정 배포
        //$("#selectedCompanys").text("회사선택");
        $("#name").removeAttr("disabled");
        $("#userID").removeAttr("disabled");
        $("#pwd").val("");
        $("#selectedCompany").text("회사선택");
        selectedCompany = {};
        mode = 0;
    }

    //저장버튼 클릭
    $("body").on('click', ".save-button", (function (e) {
        if (mode === 0)
            save(e);
        else
            update(e);
    }));

    //KPI Item 클릭
    $("body").on('click', ".items", (function () {
        bindKPI(this);
    }));

    //지표등록
    $("body").on('click', "#newButton", (function () {
        mode = 0;
        $("#saveButton").text("저장");

        reset();
    }));

    //
    //KPI Item 클릭
    $("body").on('click', ".menu-sub-item", (function () {
        var code = $(this).data("value");
        if (code === 0) {
            bindUsers(userList);
        } else {
            var nU = $.grep(userList, function (user) {
                return user.companyCode === code;
            });

            bindUsers(nU);
        }
    }));

    //KPI Item 클릭
    $("body").on('click', ".cancel-button", (function () {
        reset();
        $('.modal').modal('toggle');

    }));

    //삭제버튼 클릭
    $("body").on('click', ".delete-button", (function () {
        if (confirm("정말로 삭제하시겠습니까?")) {
            var id = $(this).data("id");
            reset();
            $('.modal').modal('toggle');
            deleteKPI();
        };
    }));

    //카테고리 버튼 클릭
    $("body").on('click', ".cItem", (function () {
        selectedCompany = {
            name: $(this).text(),
            id: $(this).attr("id")
        };

        $("#selectedCompany").text(selectedCompany.name);
    }));


    getCompanies();
    getData();
    loadHeader("../menu.html");

});