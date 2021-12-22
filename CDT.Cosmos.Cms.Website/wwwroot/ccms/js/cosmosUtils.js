// Cosmos CMS utility functions

// Automatically creates a Table of Contents for a given page
var cosmosBuildTOC = function (targetDivId, startTitle, ordByPubDate, pageNo, pageSize,) {

    if (startTitle === null || typeof (startTitle) === "undefined" || startTitle === "") {
        startTitle = document.title;
    }
    if (ordByPubDate === null || typeof (ordByPubDate) === "undefined") {
        ordByPubDate = false;
    } else {
        ordByPubDate = Boolean(ordByPubDate);
    }
    document.addEventListener("DOMContentLoaded", function () {

        cosmosGetTOC(startTitle, ordByPubDate, pageNo, pageSize, function (data) {
            var html = "<ul>";
            data.Items.forEach(function (item) {
                html += "<li><a href='/" + item.UrlPath + "'>" + item.Title.substring(item.Title.indexOf("/") + 1) + "</a></li>";
            });
            html += "</ul>";
            var el = document.getElementById(targetDivId);
            el.innerHTML = html;

            var footer = "<div>";

            if (data.PageNo > 0) {
                footer += "<span onclick=''>Prev</span>";
            }
            var current = (data.PageNo * data.PageSize) + data.PageSize;

            if (current < data.TotalCount) {
                footer += "<span onclick='' style='float:right'>Next</span>";
            }

            footer += "</div>";
        });
    });
}

// Cosmos CMS Table of Contents generator
function cosmosGetTOC(parentTitle, orderbypub, pageNo, pageSize, callback) {

    if (pageNo === null) {
        pageNo = 0;
    }

    if (pageSize === null) {
        pageSize = 10;
    }

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
    // pageNo, pageSize
    http_request.open("GET", "/Home/GetTOC/" + encodeURIComponent(parentTitle) + "?orderByPub=" + orderbypub + "&pageNo=" + pageNo + "&pageSize=" + pageSize, true);
    http_request.send();
}