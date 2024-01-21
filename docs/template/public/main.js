export default {
  iconLinks: [
    {
      icon: "github",
      href: "https://github.com/dalyIsaac/Whim",
      title: "GitHub",
    },
    {
      icon: "discord",
      href: "https://discord.gg/gEFq9wr7jb",
      title: "Discord",
    },
  ],
  start: () => {
    /**
     * @param {KeyboardEvent} e
     * @returns {void}
     */
    function listener(e) {
      if (e.key === "/") {
        const searchElement = document.getElementById("search-query");

        if (
          searchElement === null ||
          document.activeElement === searchElement
        ) {
          return;
        }

        e.preventDefault();
        searchElement.focus();
      }

      if (e.key === "f") {
        const filterElement = document.querySelector(
          "input[placeholder='Filter by title']"
        );

        if (
          filterElement === null ||
          document.activeElement === filterElement
        ) {
          return;
        }

        e.preventDefault();
        filterElement.focus();
      }
    }

    document.addEventListener("keydown", listener);
  },
};
