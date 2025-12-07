
//Home Scroll
document.querySelectorAll('.scroll-btn').forEach(button => {
    button.addEventListener('click', function(e) {
        e.preventDefault();
        const targetId = this.getAttribute('href');
        const target = document.querySelector(targetId);
        if (target) {
            const yOffset = -window.innerHeight / 2 + target.offsetHeight / 2;
            const y = target.getBoundingClientRect().top + window.pageYOffset + yOffset;
            window.scrollTo({ top: y, behavior: 'smooth' });
        }
    });
});

//ep
function changeEpisode(vdoUrl, vdoName, epFilePath) {
    document.getElementById("mainVideo").src = vdoUrl;

    document.getElementById("videoTitle").textContent = vdoName;

    document.getElementById("epAttachmentBtn").href = epFilePath;
}



function goToMyCourse(event) {
    event.preventDefault();

    const hasCourse = document.body.getAttribute("data-has-course") === "true";

    if (hasCourse) {
        window.location.href = "/cUserPersp/MyCourse";
    } else {
        if (Notification.permission === "granted") {
            new Notification("Please enroll at least one course.");
        } else if (Notification.permission !== "denied") {
            Notification.requestPermission().then(permission => {
                if (permission === "granted") {
                    new Notification("Please enroll at least one course.");
                } else {
                    alert("Please enroll at least one course.");
                }
            });
        } else {
            alert("Please enroll at least one course.");
        }
    }
}

document.addEventListener("DOMContentLoaded", function () {
    const hasCourse = @((ViewBag.HasCourse ?? false).ToString().ToLower());

    if (!hasCourse) {
        document.querySelectorAll(".check-course-before-nav")
            .forEach(link => {
                link.addEventListener("click", function (e) {
                    e.preventDefault();

                    var modal = new bootstrap.Modal(document.getElementById('noCourseModal'));
                    modal.show();
                });
            });
    }
});
