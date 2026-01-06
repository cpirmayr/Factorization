window.MathJax = {
  tex: {
    inlineMath: [['\\(', '\\)']], // Sucht nach \( ... \) für Inline-Formeln
    displayMath: [['\\[', '\\]']]  // Sucht nach \[ ... \] für Block-Formeln
  },
  options: {
    skipHtmlTags: ['script', 'noscript', 'style', 'textarea', 'pre']
  }
};

(function () {
  var script = document.createElement('script');
  script.src = 'https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js';
  script.async = true;
  document.head.appendChild(script);
})();