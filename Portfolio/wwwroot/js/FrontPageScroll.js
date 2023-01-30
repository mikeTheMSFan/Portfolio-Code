'use strict';

const about = document.getElementById('aboutMeLink');
const portfolio = document.getElementById('portfolioLink');
const contactMe = document.getElementById('contact-button');

about.addEventListener('click', () => {
  doScrolling('#about-me', 1000);
});

portfolio.addEventListener('click', () => {
  doScrolling('#portfolio', 1000);
});

contactMe.addEventListener('click', () => {
  doScrolling('#contact-me', 1000);
});
