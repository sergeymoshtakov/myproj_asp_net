// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.getElementById("submitRegister").addEventListener("click", async e => {
    e.preventDefault();
    // отправляем запрос на регистрацию
    const response = await fetch("/register", {
        method: "POST",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({
            email: document.getElementById("registerEmail").value,
            password: document.getElementById("registerPassword").value
        })
    });

    // если запрос прошел нормально
    if (response.ok === true) {
        const data = await response.json();
        alert(data.message);
    } else {
        // если произошла ошибка, получаем код статуса и сообщение
        const errorData = await response.json();
        console.log("Status: ", response.status);
        alert(errorData.message);
    }
});
