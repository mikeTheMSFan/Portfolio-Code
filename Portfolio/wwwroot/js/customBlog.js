$(document).ready(function () {
  const categoryList = document.getElementById('CategoryList');
  const addBtn = document.querySelector('[name=Add]');
  const deleteBtn = document.querySelector('[name=delete]');
  const categoryEntry = document.getElementById('CategoryEntry');
  let index = 0;

  //add category...
  addBtn.addEventListener('click', () => {
    const searchResult = search(categoryEntry.value.toLowerCase());
    if (searchResult != null) {
      swalWithDarkButton.fire({
        html: `<span class='fw-bolder'>${searchResult.toUpperCase()}</span>`,
      });
    } else {
      let newOption = new Option(categoryEntry.value.toLowerCase(), categoryEntry.value.toLowerCase(), false);

      let positionOption = null;
      if (categoryList.options.length > 0) {
        positionOption = categoryList.options[0];
      }

      for (let i = 0; i < categoryList.options.length; i++) {
        if (categoryList.options[i].text === categoryEntry.value.toLowerCase()) {
          swalWithDarkButton.fire({
            html: `<span class="fw-bolder">THIS CATEGORY EXISTS, PLEASE CHOOSE ANOTHER NAME.</span>`,
          });
          newOption = null;
        }
      }

      if (newOption) {
        categoryList.add(newOption, positionOption);
        sortSelectOptions(categoryList);
      }
    }

    categoryEntry.value = '';
    return true;
  });

  //delete Category
  deleteBtn.addEventListener('click', () => {
    if (window.location.pathname === '/Blogs/Create') {
      const category = categoryList.selectedIndex === -1 ? null : categoryList.options[categoryList.selectedIndex].value.toString();

      if (category === null) {
        swalWithDarkButton.fire({
          html: `<span class="fw-bolder">CHOOSE A CATEGORY BEFORE DELETING.</span>`,
        });
        return true;
      }
      deleteCategory();
    } else if (window.location.pathname.includes('/Blogs/Edit')) {
      const currentProtocol = window.location.protocol; // Http
      const currentHost = window.location.host; // Domain
      const category = categoryList.selectedIndex === -1 ? null : categoryList.options[categoryList.selectedIndex].value.toString();
      const blogId = document.getElementById('BlogId').value;
      const fullUrl = `${currentProtocol}//${currentHost}/Blogs/DeleteCategory/${category}/Blog/${blogId}`;
      const urlIsFound = UrlExists(fullUrl);

      if (categoryList.selectedIndex === -1) {
        swalWithDarkButton.fire({
          html: `<span class="fw-bolder">CHOOSE A CATEGORY BEFORE DELETING.</span>`,
        });
        return true;
      } else if (urlIsFound == true) {
        window.location.assign(fullUrl);
      }

      if (urlIsFound == false) {
        deleteCategory();
      }
    }
  });

  $('form').on('submit', () => {
    $('#CategoryList option').prop('selected', 'selected');
  });

  function deleteCategory() {
    let categoryCount = 1;
    while (categoryCount > 0) {
      if (categoryList.selectedIndex >= 0) {
        categoryList.options[categoryList.selectedIndex] = null;
        sortSelectOptions(categoryList);
        --categoryCount;
      } else {
        categoryCount = 0;
      }
      index--;
    }
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

  function UrlExists(url) {
    const http = new XMLHttpRequest();
    http.open('GET', url, false);
    http.send();

    if (http.status !== 404) return true;
    else {
      console.log('404 alert expected and can be safely ignored.');
      return false;
    }
  }

  function search(str) {
    if (str === '') {
      return 'EMPTY CATEGORIES ARE NOT PERMITTED.';
    }

    if (categoryList) {
      const options = categoryList.options;
      for (let index = 0; index < options.length; index++) {
        if (options[index].value === str) {
          return `THIS CATEGORY EXISTS, PLEASE CHOOSE ANOTHER NAME.`;
        }
      }
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
});
