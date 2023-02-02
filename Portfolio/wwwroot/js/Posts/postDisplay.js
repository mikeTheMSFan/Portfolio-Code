const articleData = document.getElementById('CurrentPostJson').value;
const articleDataObj = JSON.parse(articleData);

const editor = new EditorJS({
  holderId: 'editorJs',
  tools: {
    image: {
      class: ImageTool,
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
  data: articleDataObj,
  readOnly: true,
  minHeight: 0,
});
