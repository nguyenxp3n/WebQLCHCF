(function () {
    const app = window.WebQLCHCF = window.WebQLCHCF || {};

    function getEditor(source) {
        return source?.closest?.(".recipe-editor") || document.querySelector(".recipe-editor");
    }

    function getParts(editor) {
        if (!editor) {
            return {};
        }

        return {
            rows: editor.querySelector("#recipe-rows"),
            template: editor.querySelector("#recipe-row-template")
        };
    }

    function reindexRows(editor) {
        const { rows } = getParts(editor);
        if (!rows) {
            return;
        }

        Array.from(rows.querySelectorAll(".recipe-editor-row")).forEach((row, index) => {
            const number = row.querySelector(".recipe-row-index");
            if (number) {
                number.textContent = String(index + 1);
            }

            const ingredient = row.querySelector(".recipe-ingredient-select");
            if (ingredient) {
                ingredient.name = `Items[${index}].MaNL`;
                ingredient.id = `Items_${index}__MaNL`;
            }

            const quantity = row.querySelector(".recipe-quantity-input");
            if (quantity) {
                quantity.name = `Items[${index}].DinhLuong`;
                quantity.id = `Items_${index}__DinhLuong`;
            }
        });
    }

    function addIngredientRow(button) {
        const editor = getEditor(button);
        const { rows, template } = getParts(editor);
        if (!editor || !rows || !template) {
            return false;
        }

        rows.appendChild(template.content.cloneNode(true));
        reindexRows(editor);
        return true;
    }

    function removeIngredientRow(button) {
        const editor = getEditor(button);
        const row = button.closest(".recipe-editor-row");
        if (!editor || !row) {
            return false;
        }

        row.remove();
        reindexRows(editor);
        return true;
    }

    app.initRecipeEditor = function (container) {
        const scope = container || document;
        const editors = scope.matches?.(".recipe-editor")
            ? [scope]
            : Array.from(scope.querySelectorAll?.(".recipe-editor") || []);
        editors.forEach(reindexRows);
    };

    if (window.__qlchcfRecipeEditorDelegated !== true) {
        document.addEventListener("click", function (event) {
            const addButton = event.target.closest?.("#add-recipe-row, [data-recipe-add]");
            if (addButton) {
                event.preventDefault();
                addIngredientRow(addButton);
                return;
            }

            const removeButton = event.target.closest?.(".recipe-remove-row, [data-recipe-remove]");
            if (removeButton) {
                event.preventDefault();
                removeIngredientRow(removeButton);
            }
        });

        document.addEventListener("submit", function (event) {
            const editor = event.target.closest?.(".recipe-editor");
            if (editor) {
                reindexRows(editor);
            }
        });

        window.__qlchcfRecipeEditorDelegated = true;
    }

    app.initRecipeEditor(document);
})();
