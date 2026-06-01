// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM fully loaded and parsed');

    // 1. Smooth Scrolling for Anchor Links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();

            document.querySelector(this.getAttribute('href')).scrollIntoView({
                behavior: 'smooth'
            });
        });
    });

    // 2. Example: Add a simple interactive element (e.g., a "Back to Top" button)
    //    You would need to add a button with id="backToTopBtn" in your HTML for this to work.
    const backToTopBtn = document.getElementById('backToTopBtn');
    if (backToTopBtn) {
        window.addEventListener('scroll', () => {
            if (window.pageYOffset > 300) { // Show button after scrolling down 300px
                backToTopBtn.style.display = 'block';
            } else {
                backToTopBtn.style.display = 'none';
            }
        });

        backToTopBtn.addEventListener('click', () => {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });
    }

    // 3. Placeholder for other UI/UX enhancements:
    //    - Dynamic form validation
    //    - AJAX calls for partial page updates
    //    - Interactive tables (sorting, filtering)
    //    - Modals, tooltips, dropdowns (if not using a framework like Bootstrap JS)
    //    - Any custom animations or interactive components

    // Example: Simple alert on button click (if you have a button with class 'my-button')
    // document.querySelectorAll('.my-button').forEach(button => {
    //     button.addEventListener('click', function() {
    //         alert('Button clicked!');
    //     });
    // });
});
