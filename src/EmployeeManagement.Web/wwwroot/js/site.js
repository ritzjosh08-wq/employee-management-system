// Show success/error messages that the server put in TempData.
document.querySelectorAll(".toast").forEach((el) => bootstrap.Toast.getOrCreateInstance(el).show());

// Submit the employee filter form as soon as a drop-down changes.
document.querySelectorAll("[data-auto-submit]").forEach((el) =>
    el.addEventListener("change", () => el.form.requestSubmit())
);
