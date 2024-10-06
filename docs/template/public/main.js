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
     * @param {KeyboardEvent} event
     * @returns {void}
     */
    function listener(event) {
      // Ignore keypresses with modifiers.
      if (event.ctrlKey || event.metaKey || event.altKey || event.shiftKey) {
        return;
      }

      const searchElement = document.getElementById("search-query");
      const filterElement = document.querySelector(
        "input[placeholder='Filter by title']"
      );

      // If either element is already focused, ignore.
      if (
        document.activeElement === searchElement ||
        document.activeElement === filterElement
      ) {
        if (event.key === "Escape") {
          document.activeElement.blur();
        }

        return;
      }

      // Try focus the search elements.
      if (event.key === "/") {
        focusElement(event, searchElement);
      } else if (event.key === "f") {
        focusElement(event, filterElement);
      }
    }

    /**
     * @param {KeyboardEvent} event
     * @param {HTMLElement | null} element
     * @returns
     */
    function focusElement(event, element) {
      if (element === null) {
        return;
      }

      event.preventDefault();
      element.focus();
    }

    function changeSearchPlaceholder() {
      const searchElement = document.getElementById("search-query");

      if (searchElement === null) {
        return;
      }

      searchElement.placeholder = "Search /";
    }

    document.addEventListener("keydown", listener);
    changeSearchPlaceholder();
  },
};
