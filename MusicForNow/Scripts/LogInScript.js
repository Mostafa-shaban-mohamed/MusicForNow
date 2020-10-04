$(document).ready(function () {
    $('#LogInBtn').on('click', function () {
        var url = '/Home/LogIn';
        var options = { "backdrop": "static", keyboard: true };
        $.ajax({
            type: "GET",
            url: url,
            contentType: "application/json; charset=utf-8",
            datatype: "json",
            success: function (data) {
                $('#myModalContent').html(data);
                $('#myModal').modal(options);
                $('#myModal').modal('show');

            },
            error: function () {
                alert("Dynamic content load failed.");
            }
        });
    });
    $("#closbtn").click(function () {
        $('#myModal').modal('hide');
    });
});