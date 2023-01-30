const alert = document.querySelector('.alert');

$(alert).click(function () {
  $(alert)
    .fadeTo(50, 700)
    .slideUp(500, () => {
      $(alert).alert('close');
    });
});

//if not clicked...close after 10 seconds
setTimeout(() => {
  $(alert)
    .fadeTo(50, 700)
    .slideUp(500, () => {
      $(alert).alert('close');
    });
}, 10000);
