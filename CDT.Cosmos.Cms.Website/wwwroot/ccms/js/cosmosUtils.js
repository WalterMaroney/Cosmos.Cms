// Cosmos CMS utility functions

var cosmosBuildTOC = function (targetDivId) {
    document.addEventListener("DOMContentLoaded", function () {
        cosmosGetTOC(document.title, true, function (data) {
            var html = "<ul>";
            data.forEach(function (item) {
                html += "<li><a href='/" + item.UrlPath + "'>" + item.Title.substring(item.Title.indexOf("/") + 1) + "</a></li>";
            });
            html += "</ul>";
            var el = document.getElementById(targetDivId);
            el.innerHTML = html;
        });
    });
}

function cosmosGetTOC(parentTitle, orderbypub, callback) {
    var http_request = new XMLHttpRequest();
    try {
        // Opera 8.0+, Firefox, Chrome, Safari
        http_request = new XMLHttpRequest();
    } catch (e) {
        // Internet Explorer Browsers
        try {
            http_request = new ActiveXObject("Msxml2.XMLHTTP");

        } catch (e) {

            try {
                http_request = new ActiveXObject("Microsoft.XMLHTTP");
            } catch (e) {
                // Something went wrong
                alert("Your browser broke!");
                return false;
            }
        }
    }

    http_request.onreadystatechange = function () {
        if (http_request.readyState == 4) {
            // Javascript function JSON.parse to parse JSON data
            var data = JSON.parse(http_request.responseText);
            callback(data);
        }
    }

    http_request.open("GET", "/Home/GetTOC/" + encodeURIComponent(parentTitle) + "?orderByPub=" + orderbypub, true);
    http_request.send();
}