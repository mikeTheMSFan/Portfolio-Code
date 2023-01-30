const imagebox = document.getElementById('image-box');
const crop_btn = document.getElementById('crop-btn');
const input = document.getElementById('ImageFile');

input.addEventListener('change', () => {
    $('#slider').show();
    const img_data = input.files[0];
    const url = URL.createObjectURL(img_data);

    imagebox.innerHTML = `<img src="${url}" id="image" style="width:100%;">`;

    const image = document.getElementById('image');

    document.getElementById('image-box').style.display = 'block';
    document.getElementById('crop-btn').style.display = 'block';
    document.getElementById('confirm-btn').style.display = 'none';

    let imageWidth = 0;
    let imageHeight = 0;
    if (location.href.indexOf('Blogs')) {
        imageWidth = 1920;
        imageHeight = 390;
    }

    if (location.href.indexOf('Posts')) {
        imageWidth = 1600;
        imageHeight = 718;
    }

    console.log(imageHeight);

    cropper = new Cropper(image, {
        autoCropArea: 1,
        toggleDragModeOnDblclick: false,
        viewMode: 1,
        zoomable: true,
        movable: false,
        cropBoxResizable: false,
        dragMode: 'move',
        aspectRatio: imageWidth / imageHeight,
        minCropBoxWidth: 200,
        minCropBoxHeight: 86,
    });

    crop_btn.addEventListener('click', () => {
        resetSlideBar();
        cropper
            .getCroppedCanvas({
                width: imageWidth,
                height: imageHeight,
            })
            .toBlob((blob) => {
                let fileInputElement = document.getElementById('ImageFile');

                let file = new File([blob], img_data.name, {
                    type: 'image/*',
                    lastModified: new Date().getTime(),
                });

                let container = new DataTransfer();
                container.items.add(file);
                fileInputElement.files = container.files;

                document.getElementById('image-box').style.display = 'none';
                document.getElementById('crop-btn').style.display = 'none';
                document.getElementById('confirm-btn').style.display = 'block';
            });
    });
});

input.addEventListener('change', resetSlideBar);

$(function () {
    let zoomRatio = 0;

    $('#slider').slider({
        range: 'min',
        min: 0,
        max: 1,
        step: 0.1,
        slide: function (event, ui) {
            let slideVal = ui.value;
            let zoomRatio = Math.round((slideVal - slideValGlobal) * 10) / 10;
            slideValGlobal = slideVal;
            cropper.zoom(zoomRatio);
        },
    });
    resetSlideBar();
});

function resetSlideBar() {
    slideValGlobal = 0;
    $('#slider').slider('value', slideValGlobal);
}