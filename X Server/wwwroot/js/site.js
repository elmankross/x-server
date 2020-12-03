window.onload = function () {
    const t = localStorage.getItem("refreshTime")
    if (t != null) {
        setAutoRefresh(t);
    }
}


function setAutoRefresh(ms) {

    const button = $("#dropdownRefreshTime")[0];
    if (ms == -1) {
        localStorage.removeItem("refreshTime");
        button.innerText = "Refresh every (None)";
        return;
    }

    localStorage.setItem("refreshTime", ms);
    button.innerText = "Refresh every (" + ms / 1000 + "s)";
    setTimeout(function () {
        location.reload(true);
    }, ms);
}