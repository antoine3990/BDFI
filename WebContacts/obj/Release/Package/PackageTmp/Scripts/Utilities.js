﻿/////////////////////////////////////////////////////////////////////////////////////
//  Utilitaires pour le site
//  Auteur: Nicolas Chourot
/////////////////////////////////////////////////////////////////////////////////////

$(".datepicker").datepicker({format: 'yyyy-mm-dd'});
$("#Phone").mask("(999) 999-9999");
$("#ImageUploader").change(function (e) { PreLoadImage(e); })
$("#UploadButton").click(function () { $("#ImageUploader").trigger("click"); })

function PreLoadImage(e) {
    // Saisir la référence sur l'image cible
    var imageTarget = $("#UploadedImage")[0];
    // Saisir la référence sur le fileUpload
    var input = $("#ImageUploader")[0];

    if (imageTarget != null) {
        var len = input.value.length;

        if (len != 0) {
            var fname = input.value;
            var ext = fname.substr(len - 3, len).toLowerCase();

            if ((ext != "png") &&
                (ext != "jpg") &&
                (ext != "bmp") &&
                (ext != "gif")) {
                bootbox.alert("Ce n'est pas un fichier d'image de format reconnu. Sélectionnez un autre fichier.");
            }
            else {
                var fReader = new FileReader();
                fReader.readAsDataURL(input.files[0]);
                fReader.onloadend = function (event) {
                    // event.target.result contiens les données de l'image
                    imageTarget.src = event.target.result;
                }
            }
        }
        else {
            imageTarget.src = null;
        }
    }
    return true;
}

$("a.confirm").click(function (e) {
    e.preventDefault(); // désactive la passation de l'événement
    var href = $(this).attr("href");
    var message = $(this).attr("message");
    bootbox.confirm(message, function (confirmResult) {
        if (confirmResult) {
            // demander au fureteur d'effectuer une requête vers l'url href
            window.location.href = href;
        }
    });
});


$("#MoveLeft").on('click', function () {
    var movingVal = $('#ContactsList option:selected').val();
    var movingItem = $('#ContactsList option:selected').text();
    if (movingItem != "") {
        $('#ContactsList option:selected').remove();
        var o = new Option(movingItem, movingVal);
        $('#FriendsList').append(o);
        SortSelect("FriendsList");
    }
})
$("#MoveRight").on('click', function () {
    var movingVal = $('#FriendsList option:selected').val();
    var movingItem = $('#FriendsList option:selected').text();
    if (movingItem != "") {
        $('#FriendsList option:selected').remove();
        var o = new Option(movingItem, movingVal);
        $('#ContactsList').append(o);
        SortSelect("ContactsList");
    }
})

$("#save").on('click', function (event) {
    var friends_Id_String_List = "";
    $('#FriendsList option').each(function () {
        friends_Id_String_List += this.value + ",";
    });
    $("#FriendsListItems").val(friends_Id_String_List);
});

function SortSelect(selectId) {
    $("#" + selectId).html($("#" + selectId + " option").sort(function (a, b) {
        return a.text == b.text ? 0 : a.text < b.text ? -1 : 1
    }))
}

