(function () {
    window.addEventListener("load", function () {
        var link = document.querySelector("link[rel*='icon']") || document.createElement('link');
        var logo = document.getElementsByClassName('link');

        document.head.removeChild(link);
        link = document.querySelector("link[rel*='icon']") || document.createElement('link');
        document.head.removeChild(link);
        link = document.createElement('link');
        link.type = 'image/x-icon';
        link.rel = 'shortcut icon';
        link.href = '/docs/assets/favicon.ico';
        document.getElementsByTagName('head')[0].appendChild(link);

        logo[0].children[0].alt = "MechaChat Logo";
        logo[0].children[0].src = "/docs/assets/logo.png";
    });
})();