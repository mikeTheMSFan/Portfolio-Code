'use strict';

let url = '';
const id = $('#Project_Id').val();
if (window.location.href.indexOf('Edit') > -1) {
  url = `/Projects/Edit/?id=${id}`;
} else {
  url = '/Projects/Create';
}

Dropzone.options.myDropzone = {
  url: url,
  autoProcessQueue: false,
  uploadMultiple: true,
  parallelUploads: 5,
  maxFiles: 5,
  uploadMultiple: true,
  addRemoveLinks: true,
  acceptedFiles: 'image/*',
  paramName: ImageFiles,
  init: function () {
    const myDropzone = this;

    //add images if needed.
    checkForImages(myDropzone);

    // for Dropzone to process the queue (instead of default form behavior):
    $('#confirm-btn').click(function (e) {
      processProjectForm(e, myDropzone);
    });

    //send all the form data along with the files:
    this.on('sendingmultiple', function (data, xhr, formData) {
      submitProjectForm(formData);
    });

    this.on('success', function (file, xhr) {
      // remove all files from dropzone
      myDropzone.removeAllFiles();

      //alert user project was successfully created
      processSuccessAlert();
    });

    this.on('error', function (file, errormessage, xhr) {
      //remove all files from drop zone
      requeueFileWithError(file, this);

      //display errors to user
      displayErrors(xhr);
    });
  },

  transformFile: function (file, done) {
    // Create the image editor overlay
    const myDropZone = this;
    const editor = document.createElement('div');
    createImageEditor(editor);

    // Create an image node for Cropper.js
    const image = createImageNode(editor, file);

    //cropper Create Cropper.js
    const cropper = createCropper(image);

    // Create confirm button at bottom center of the viewport
    const buttonConfirm = createConfirmButton(editor);

    buttonConfirm.addEventListener('click', function () {
      // Get the canvas with image data from Cropper.js
      const canvas = getCanvas(cropper);

      // Turn the canvas into a Blob (file object without a name)
      canvas.toBlob(function (blob) {
        // Create a new Dropzone file thumbnail
        createDropzoneThumbnail(myDropZone, blob, file, done);

        // Remove the editor from the view
        document.body.removeChild(editor);
      });
    });
  },
};

function ImageFiles() {
  return 'ImageFiles';
}

function requeueFileWithError(file, myDropZone) {
  file.previewElement.classList.remove('dz-processing');
  file.previewElement.classList.remove('dz-error');
  file.previewElement.classList.remove('dz-complete');
  file.status = 'Dropzone.QUEUED';
  file.upload.progress = 0;
  file.upload.bytesSent = 0;
  const alteredFile = file;

  myDropZone.removeFile(file);
  myDropZone.addFile(alteredFile);
}

function processSuccessAlert() {
  let projectAction = 'created.';
  if (window.location.href.indexOf('Edit') > -1) {
    projectAction = 'Edited.';
  }
  swalWithBulmaButton
    .fire({
      html: `<h3>Success</h3><br /><p>Your project has been ${projectAction}</p>`,
    })
    .then(() => {
      location.href = '/';
    });
}

function checkForImages(myDropZone) {
  if (window.location.href.indexOf('Edit') > -1) {
    const projectImages = document.getElementById('projectImages').value;
    const projectImagesObj = JSON.parse(projectImages);

    //sort objects by name
    // order an array of objects with name
    projectImagesObj.sort(function (a, b) {
      if (a.Name < b.Name) {
        return -1;
      }
      if (a.Name > b.Name) {
        return 1;
      }
      return 0;
    });

    console.log(projectImagesObj);

    for (let i = 0; i < projectImagesObj.length; i++) {
      const base64Image = projectImagesObj[i].Image;
      const blob = base64ToBlob(base64Image, 'image/png');
      blob.name = `${projectImagesObj[i].Name}.png`;
      myDropZone.addFile(blob);
    }
  }
}

function processProjectForm(event, dzClosure) {
  const validator = $('#projectForm').validate();
  validator.form();

  if (validator.numberOfInvalids() >= 1) {
    //...do nothing.
  } else if (validator.numberOfInvalids() === 0) {
    event.preventDefault();
    event.stopPropagation();
    dzClosure.processQueue();
  }
}

function checkIfStringIsJSON(xhr) {
  let isJSON = true;
  try {
    const json = JSON.parse(xhr.response);
  } catch (e) {
    isJSON = false;
  }
  return isJSON;
}

function displayErrors(xhr) {
  if (xhr) {
    if (xhr.readyState === 4) {
      //check if string is JSON string
      const isJSON = checkIfStringIsJSON(xhr);
      if (isJSON) {
        const modelStateErrors = JSON.parse(xhr.response);
        //get element based on key and insert error
        modelStateErrors.forEach((modelStateError) => {
          const element = document.querySelector(`[data-valmsg-for="${modelStateError.key}"]`);
          element.innerHTML = '';
          element.innerHTML = modelStateError.message;
        });
      } else {
        console.log(xhr.response);
      }
    }
  }
}

function submitProjectForm(formData) {
  formData.append('__RequestVerificationToken', $(':input[name=' + '__RequestVerificationToken' + ']').val());
  formData.append('Project.Categories', $('#Project_Categories').val());
  formData.append('Project.Title', $('#Project_Title').val());
  formData.append('Project.Description', $('#Project_Description').val());
  formData.append('Project.ProjectUrl', $('#Project_ProjectUrl').val());

  if (window.location.href.indexOf('Edit') > -1) {
    formData.append('Project.Id', $('#Project_Id').val());
  }
}

function createImageEditor(editor) {
  editor.style.position = 'fixed';
  editor.style.left = 0;
  editor.style.right = 0;
  editor.style.top = 0;
  editor.style.bottom = 0;
  editor.style.zIndex = 9999;
  editor.style.backgroundColor = '#000';
  document.body.appendChild(editor);
}

function createImageNode(editor, file) {
  const image = new Image();
  image.src = URL.createObjectURL(file);
  editor.appendChild(image);
  return image;
}

function createConfirmButton(editor) {
  const buttonConfirm = document.createElement('button');
  buttonConfirm.style.position = 'absolute';
  buttonConfirm.style.marginLeft = 'auto';
  buttonConfirm.style.marginRight = 'auto';
  buttonConfirm.style.width = '100px';
  buttonConfirm.style.left = '0px';
  buttonConfirm.style.right = '0px';
  buttonConfirm.style.top = '90%';
  buttonConfirm.style.zIndex = 9999;
  buttonConfirm.classList.add('btn');
  buttonConfirm.classList.add('btn-primary');
  buttonConfirm.textContent = 'Confirm';
  editor.appendChild(buttonConfirm);

  return buttonConfirm;
}

function createCropper(image) {
  const cropper = new Cropper(image, {
    autoCropArea: 1,
    toggleDragModeOnDblclick: false,
    viewMode: 2,
    zoomable: false,
    movable: false,
    cropBoxResizable: false,
    dragMode: 'move',
    aspectRatio: 1140 / 758,
    minCropBoxWidth: 1140,
    minCropBoxHeight: 758,
  });

  return cropper;
}

function getCanvas(cropper) {
  const canvas = cropper.getCroppedCanvas({
    width: 1140,
    height: 758,
  });

  return canvas;
}

function createDropzoneThumbnail(dropZone, blob, file, done) {
  dropZone.createThumbnail(
    blob,
    dropZone.options.thumbnailWidth,
    dropZone.options.thumbnailHeight,
    dropZone.options.thumbnailMethod,
    false,
    function (dataURL) {
      // Update the Dropzone file thumbnail
      dropZone.emit('thumbnail', file, dataURL);
      // Return the file to Dropzone
      done(blob);
    }
  );
}

const swalWithBulmaButton = Swal.mixin({
  imageUrl: '/imgs/project/success.png',
});
