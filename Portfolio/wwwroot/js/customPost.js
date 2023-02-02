$(document).ready(function () {
  const tagList = document.getElementById('TagList');
  const addBtn = document.querySelector('[name=Add]');
  const deleteBtn = document.querySelector('[name=delete]');
  const tagEntry = document.getElementById('TagEntry');
  const categorySelectList = document.getElementById('Post_CategoryId');
  const categoryId = document.getElementById('CategoryId');
  const blogSelectList = document.getElementById('Post_BlogId');
  const form = document.getElementById('postForm');
  const articleData = document.getElementById('ArticleData').value;
  const host = location.protocol.concat('//').concat(window.location.host);

  let articleDataJson = null;
  if (articleData) {
    articleDataJson = JSON.parse(articleData);
  }

  let index = 0;

  jQuery.validator.setDefaults({
    ignore: ":hidden, [contenteditable='true']:not([name])",
  });

  fetchCategories();
  blogSelectList.addEventListener('change', () => {
    fetchCategories();
  });

  //add tag...
  addBtn.addEventListener('click', () => {
    const searchResult = search(tagEntry.value.toLowerCase());
    if (searchResult != null) {
      swalWithDarkButton.fire({
        html: `<span class='fw-bolder'>${searchResult.toUpperCase()}</span>`,
      });
    } else {
      let newOption = new Option(tagEntry.value.toLowerCase(), tagEntry.value.toLowerCase(), false);

      let positionOption = null;
      if (tagList.options.length > 0) {
        positionOption = tagList.options[0];
      }

      for (let i = 0; i < tagList.options.length; i++) {
        if (tagList.options[i].text === tagEntry.value.toLowerCase()) {
          swalWithDarkButton.fire({
            html: `<span class="fw-bolder">THIS TAG EXISTS, PLEASE CHOOSE ANOTHER NAME.</span>`,
          });
          newOption = null;
        }
      }

      if (newOption) {
        tagList.add(newOption, positionOption);
        sortSelectOptions(tagList);
      }
    }

    tagEntry.value = '';
    return true;
  });

  //delete tag
  deleteBtn.addEventListener('click', () => {
    const tag = tagList.selectedIndex === -1 ? null : tagList.options[tagList.selectedIndex].value.toString();

    if (tag === null) {
      swalWithDarkButton.fire({
        html: `<span class="fw-bolder">CHOOSE A TAG BEFORE DELETING.</span>`,
      });
      return true;
    }
    deleteTag();
  });

  $('form').on('submit', () => {
    $('#TagList option').prop('selected', 'selected');
  });

  function deleteTag() {
    let tagCount = 1;
    while (tagCount > 0) {
      if (tagList.selectedIndex >= 0) {
        tagList.options[tagList.selectedIndex] = null;
        //value to break loop
        --tagCount;
      } else {
        tagCount = 0;
      }
      //decrement index
      index--;
    }
  }

  function search(str) {
    if (str === '') {
      return 'EMPTY TAGS ARE NOT PERMITTED.';
    }

    if (tagList) {
      const options = tagList.options;
      for (let index = 0; index < options.length; index++) {
        if (options[index].value == str) {
          return `THIS TAG EXISTS, PLEASE CHOOSE ANOTHER NAME.`;
        }
      }
    }
  }

  function fetchCategories() {
    const blogId = blogSelectList.value;
    const fetchRes = fetch(`/Blogs/GetCategories/${blogId}`);

    fetchRes
      .then((result) => result.json())
      .then((result) => {
        if (categorySelectList != null) {
          categorySelectList.innerHTML = '';
          for (let i = 0; i < result.categoryListJson.length; i++) {
            const option = document.createElement('option');
            option.value = result.categoryListJson[i].id;
            option.innerHTML = result.categoryListJson[i].name;
            categorySelectList.appendChild(option);
          }
        }

        if (categoryId?.value !== undefined) {
          categorySelectList.value = categoryId.value;
        }
      });
  }

  function sortSelectOptions(selectElement) {
    var options = Array.from(selectElement.children);

    options.sort(function (a, b) {
      if (a.text.toUpperCase() > b.text.toUpperCase()) return 1;
      else if (a.text.toUpperCase() < b.text.toUpperCase()) return -1;
      else return 0;
    });

    selectElement.options = null;
    for (let i = 0; i < options.length; i++) {
      selectElement.add(options[i]);
    }
  }

  const swalWithDarkButton = Swal.mixin({
    customClass: {
      confirmButton: 'btn btn-danger btn-sm w-100',
    },
    imageUrl: '/imgs/error.png',
    timer: 5000,
    buttonsStyling: false,
  });

  const editor = new EditorJS({
    holder: 'editorJs',
    tools: {
      image: {
        class: ImageTool,
        config: {
          endpoints: {
            byFile: `${host}/Posts/UploadPostImageFile`,
          },
          field: 'file',
        },
      },
      paragraph: {
        class: Paragraph,
        inlineToolbar: true,
      },
      list: {
        class: List,
        inlineToolbar: true,
        config: {
          defaultStyle: 'unordered',
        },
      },
      header: Header,
      quote: Quote,
      code: CodeTool,
    },
    minHeight: 0,
    data: articleDataJson,
  });

  form.addEventListener('submit', (e) => {
    editor
      .save()
      .then((outputData) => {
        const postContent = document.getElementById('Post_Content');
        postContent.value = outputData.blocks.length === 0 ? '' : JSON.stringify(outputData);
      })
      .catch((error) => {
        alert('An error occured, please let the administrator know.');
        false;
      });
  });
});
