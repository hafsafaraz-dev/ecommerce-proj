document.addEventListener('DOMContentLoaded', function () {
  var navToggle = document.getElementById('navToggle');
  var navDrawer = document.getElementById('navDrawer');
  if (navToggle && navDrawer) {
    navToggle.addEventListener('click', function () {
      navDrawer.classList.toggle('open');
    });
  }

  var sidebarToggle = document.getElementById('sidebarToggle');
  var adminSidebar = document.getElementById('adminSidebar');
  var sidebarOverlay = document.getElementById('sidebarOverlay');
  function toggleSidebar(open) {
    if (open !== undefined) {
      adminSidebar.classList.toggle('open', open);
      if (sidebarOverlay) sidebarOverlay.classList.toggle('visible', open);
    } else {
      adminSidebar.classList.toggle('open');
      if (sidebarOverlay) sidebarOverlay.classList.toggle('visible');
    }
  }
  if (sidebarToggle && adminSidebar) {
    sidebarToggle.addEventListener('click', function () { toggleSidebar(); });
  }
  if (sidebarOverlay) {
    sidebarOverlay.addEventListener('click', function () { toggleSidebar(false); });
  }
});
