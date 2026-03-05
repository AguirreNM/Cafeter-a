const authWrapper    = document.querySelector('.auth-wrapper');
const loginTriggers  = document.querySelectorAll('.login-trigger');
const registerTriggers = document.querySelectorAll('.register-trigger');

// Si la página carga ya en modo registro, quitamos las transiciones
// brevemente para que no se vea el estado "en tránsito".
if (authWrapper.classList.contains('toggled')) {
    authWrapper.classList.add('no-transition');
    requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            authWrapper.classList.remove('no-transition');
        });
    });
}

registerTriggers.forEach(trigger => {
    trigger.addEventListener('click', (e) => {
        e.preventDefault();
        authWrapper.classList.add('toggled');
    });
});

loginTriggers.forEach(trigger => {
    trigger.addEventListener('click', (e) => {
        e.preventDefault();
        authWrapper.classList.remove('toggled');
    });
});
