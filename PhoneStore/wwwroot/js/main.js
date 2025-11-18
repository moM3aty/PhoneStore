const header = document.querySelector('#header');
const headerHeight = header.offsetHeight;
const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
const sections = Array.from(navLinks).map(link => {
  const id = link.getAttribute('href');
  return id.startsWith('#') ? document.querySelector(id) : null;
}).filter(Boolean);

function handleScroll() {

  header.classList.toggle('sticky', window.scrollY > 50);


  const scrollPos = window.scrollY + headerHeight + 5;

  let currentSection = null;

  for (const section of sections) {
    if (section.offsetTop <= scrollPos && (section.offsetTop + section.offsetHeight) > scrollPos) {
      currentSection = section;
      break;
    }
  }

  navLinks.forEach(link => {
    if (currentSection && link.getAttribute('href') === `#${currentSection.id}`) {
      link.classList.add('active');
    } else {
      link.classList.remove('active');
    }
  });
}

window.addEventListener('scroll', handleScroll);

handleScroll();


document.querySelectorAll('a[href^="#"]').forEach(anchor => {
  anchor.addEventListener('click', function (e) {
    e.preventDefault();
    const targetId = this.getAttribute('href');
    if (targetId === '#') return;
    const targetElement = document.querySelector(targetId);
    if (!targetElement) return;

    window.scrollTo({
      top: targetElement.offsetTop - headerHeight,
      behavior: 'smooth'
    });
  });
});
const navbarToggler = document.querySelector('.navbar-toggler');
const navbarCollapse = document.querySelector('.navbar-collapse');

navLinks.forEach(link => {
  link.addEventListener('click', () => {
    if (navbarToggler && navbarCollapse.classList.contains('show')) {
      navbarToggler.click();
    }
  });
});

var swiper = new Swiper(".mySwiper", {
  slidesPerView: "auto",
  spaceBetween: 0,
  speed: 6000,
  loop: true,
  allowTouchMove: false,

  autoplay: {
    delay: 0,
    disableOnInteraction: false
  },


  direction: "horizontal"
});

AOS.init({
  offset: 120,
  duration: 600,
  delay: 0,
  once: false,
  easing: 'ease-out-cubic'
});