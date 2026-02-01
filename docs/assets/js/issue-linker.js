document.addEventListener("DOMContentLoaded", function() {
    const body = document.body;
    body.innerHTML = body.innerHTML.replace(/#(\d+)/g, `<a href="${repoUrl}/issues/$1">#$1</a>`);
});