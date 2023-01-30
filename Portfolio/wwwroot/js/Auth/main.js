'use strict';

 $(window).on("load", function () {
     if ($('.form-sent', '.form-content')) {
         $('.form-sent', '.form-content').addClass('show-it');
     }

     $('.btn-forget').on('click',function(e){
         e.preventDefault();
         const inputField = $(this).closest('form').find('input');
         if(inputField.attr('required') && inputField.val()){
             $('.error-message').remove();
         }else{
             $('.error-message').remove();
             $('<small class="error-message">Please fill the field.</small>').insertAfter(inputField);
         }
         const form = document.getElementById('forgetForm');
         form.submit();
     });

     $('.btn-tab-next').on('click',function(e){
         e.preventDefault();
         $('.nav-tabs .nav-item > .active').parent().next('li').find('a').trigger('click');
     });
     $('.custom-file input[type="file"]').on('change', function(){
         var filename = $(this).val().split('\\').pop();
         $(this).next().text(filename);
     });
 });
