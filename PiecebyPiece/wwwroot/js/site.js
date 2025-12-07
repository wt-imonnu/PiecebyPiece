// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.querySelectorAll('.subject-filter').forEach(cb => {
    cb.addEventListener('change', () => {
        document.getElementById('filterForm').submit();
    });
});

    function toggleCollapse(button) {
    const container = button.closest('.collapsible');
    container.classList.toggle('collapsed');
}

document.addEventListener('DOMContentLoaded', function () {
    var confirmButton = document.getElementById('confirmLogoutButton');

    if (confirmButton) {
        var logoutUrl = confirmButton.getAttribute('data-logout-url');

        if (logoutUrl) {
            confirmButton.href = logoutUrl;
        }
    }
});


    $(document).ready(function () {
            const antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();

            //<!-- Delete Lesson -->
    var deleteLessonModal = new bootstrap.Modal(document.getElementById('deleteLessonConfirmModal'));
    var currentLessonDeleteUrl = '';
    var redirectCourseId = 0;

    $('.btn-delete-lesson-modal').on('click', function (e) {
        e.preventDefault();

    currentLessonDeleteUrl = $(this).data('delete-url');
    var lessonId = $(this).data('lesson-id');
    var lessonName = $(this).data('lesson-name');
    redirectCourseId = $(this).data('course-id');

    $('#lessonIdToDelete').text(lessonId);
    $('#lessonNameToDelete').text(lessonName);
    deleteLessonModal.show();
            });

    $('#confirmLessonDeleteButton').on('click', function () {
                if (!currentLessonDeleteUrl) return;
    deleteLessonModal.hide();

    $.ajax({
        url: currentLessonDeleteUrl,
    type: 'POST',
    headers: {'RequestVerificationToken': antiForgeryToken },
    success: function (response) {
                        if (redirectCourseId && redirectCourseId > 0) {
        window.location.href = '/cCourse/Details/' + redirectCourseId;
                        } else {
        alert("Lesson deleted successfully, but cannot redirect to parent Course.");
                        }
                    },
    error: function (xhr, status, error) {
        alert("Error: The deletion failed. Please ensure the Lesson still has a valid parent Course ID.");
    console.error("Deletion failed:", error);
                    }
                });
            });


            //<!-- Delete Episode -->
    var deleteEpModal = new bootstrap.Modal(document.getElementById('deleteEpConfirmModal'));
    var currentEpDeleteUrl = '';
    var redirectLessonId = 0; // ตัวแปรเก็บ Lesson ID

    $('.btn-delete-ep-modal').on('click', function (e) {
        e.preventDefault();

    currentEpDeleteUrl = $(this).data('delete-url');
    var epId = $(this).data('ep-id');
    var epName = $(this).data('ep-name');
    redirectLessonId = $(this).data('lesson-id'); // เก็บ Lesson ID

    $('#epIdToDelete').text(epId);
    $('#epNameToDelete').text(epName);

    deleteEpModal.show();
            });

    $('#confirmEpDeleteButton').on('click', function () {
                if (!currentEpDeleteUrl) return;

    deleteEpModal.hide();

    $.ajax({
        url: currentEpDeleteUrl,
    type: 'POST',
    headers: {'RequestVerificationToken': antiForgeryToken }, 
    success: function (response) {
                        if (redirectLessonId > 0) {
        window.location.href = '/cLesson/Details/' + redirectLessonId;
                        } else {
        window.location.reload();
                        }
                    },
    error: function (xhr, status, error) {
        alert("Error: Deletion failed. Please contact support.");
    console.error("Deletion failed:", error);
                    }
                });
            });
        });
