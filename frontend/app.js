// Modal logic for login and signup
function showModal(id) {
    document.getElementById(id).style.display = 'flex';
}
function closeModal(id) {
    document.getElementById(id).style.display = 'none';
}

// MVC: import controllers
import('./controllers/homeController.js').then(m => window.homeController = m);
import('./controllers/initiativesController.js').then(m => window.initiativesController = m);
import('./controllers/profileController.js').then(m => window.profileController = m);
import('./controllers/projectsController.js').then(m => window.projectsController = m);
import('./controllers/historyController.js').then(m => window.historyController = m);

async function loadView(view) {
    let html = '';
    switch(view) {
        case 'home':
            html = await fetch('views/home.html').then(r => r.text());
            break;
        case 'initiatives':
            html = await fetch('views/initiatives.html').then(r => r.text());
            break;
        case 'profile':
            html = await fetch('views/profile.html').then(r => r.text());
            break;
        case 'projects':
            html = await fetch('views/projects.html').then(r => r.text());
            break;
        case 'history':
            html = await fetch('views/history.html').then(r => r.text());
            break;
        default:
            html = await fetch('views/home.html').then(r => r.text());
    }
    document.getElementById('main-content').innerHTML = html;
}

function setupNavigation() {
    document.getElementById('nav-home').onclick = () => loadView('home');
    document.getElementById('nav-initiatives').onclick = () => loadView('initiatives');
    const profile = document.getElementById('nav-profile');
    if (profile) profile.onclick = () => loadView('profile');
    const projects = document.getElementById('nav-projects');
    if (projects) projects.onclick = () => loadView('projects');
    const history = document.getElementById('nav-history');
    if (history) history.onclick = () => loadView('history');
}

// Functionality for login
function handleLogin(username, password) {
    if (username === "admin" && password === "password") {
        alert("Login successful!");
        document.getElementById("main-content").innerHTML = `<h2>Welcome, ${username}!</h2><p>You are now logged in.</p>`;
    } else {
        alert("Invalid username or password.");
    }
}

// Functionality for register
function handleRegister(username, email, password) {
    if (username && email && password) {
        alert("Registration successful! You can now log in.");
        document.getElementById("main-content").innerHTML = `<h2>Welcome, ${username}!</h2><p>Your account has been created.</p>`;
    } else {
        alert("Please fill out all fields.");
    }
}

// Event listeners for login and register forms
function setupFormHandlers() {
    const loginForm = document.getElementById("loginForm");
    const signupForm = document.getElementById("signupForm");

    if (loginForm) {
        loginForm.onsubmit = (e) => {
            e.preventDefault();
            const username = loginForm.querySelector("input[type='text']").value;
            const password = loginForm.querySelector("input[type='password']").value;
            handleLogin(username, password);
        };
    }

    if (signupForm) {
        signupForm.onsubmit = (e) => {
            e.preventDefault();
            const username = signupForm.querySelector("input[type='text']").value;
            const email = signupForm.querySelector("input[type='email']").value;
            const password = signupForm.querySelector("input[type='password']").value;
            handleRegister(username, email, password);
        };
    }
}

// Update home page dynamically
function updateHomePage() {
    const homeContent = `<section class="hero-section">
        <div class="hero-text">
            <h2>Welcome to the Volunteer Coordination Platform</h2>
            <p>Join initiatives, manage projects, and track your activities.</p>
        </div>
    </section>`;
    document.getElementById("main-content").innerHTML = homeContent;
}

document.addEventListener('DOMContentLoaded', () => {
    // Modal events
    document.getElementById('loginBtn').onclick = () => showModal('loginModal');
    document.getElementById('signupBtn').onclick = () => showModal('signupModal');
    document.getElementById('closeLogin').onclick = () => closeModal('loginModal');
    document.getElementById('closeSignup').onclick = () => closeModal('signupModal');
    // Close modal on outside click
    window.onclick = function(event) {
        if (event.target === document.getElementById('loginModal')) closeModal('loginModal');
        if (event.target === document.getElementById('signupModal')) closeModal('signupModal');
    };
    setupNavigation();
    setupFormHandlers();
    updateHomePage();
});
