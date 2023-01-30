$(document).ready(function () {
  const commentBodyText = document.getElementById('CommentBody').value;
  const commentBodyElement = document.getElementById('Comment_Body');
  const userStatus = document.getElementById('UserStatus').value;

  $(commentBodyElement).summernote({
    height: 200,
    placeholder: 'Type your comment here...',
    disableResizeEditor: true,
    toolbar: [
      ['style', ['style']],
      ['font', ['bold', 'underline', 'clear']],
      ['fontname', ['fontname']],
      ['color', ['color']],
      ['para', ['ul', 'ol', 'paragraph']],
      ['view', ['fullscreen', 'help']],
    ],
  });

  if (userStatus === 'loggedOut') {
    $(commentBodyElement).summernote('disable');
  }

  if (commentBodyText !== '') {
    $(commentBodyElement).summernote('code', commentBodyText);
  }
});
