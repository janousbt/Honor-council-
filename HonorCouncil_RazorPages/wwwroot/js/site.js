document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll("[data-click-file]").forEach(trigger => {
    trigger.addEventListener("click", () => {
      const inputId = trigger.getAttribute("data-click-file");
      const input = inputId ? document.getElementById(inputId) : null;
      input?.click();
    });
  });
});
