window.onload = function () {
    GetPersons("");
};
let userSearchInput = document.querySelector(".userSearch");
var filterOption = "name";
var orderType = "none";
var orderFilter = "Age";
function FilterBar() {

    let searchFilterBlock = document.querySelector(".searchFilerContainer");
    let searchIcon = document.querySelector(".searchIcon");
    let filterI = document.querySelectorAll(".filter");

    searchIcon.addEventListener("click", function () {

        this.classList.toggle("animate-circle");

        if (searchFilterBlock.style.opacity === '0' || searchFilterBlock.style.opacity === '') {
            searchFilterBlock.style.opacity = '1';
        } else {
            searchFilterBlock.style.opacity = '0';
        }
    });


    filterI.forEach(filter => {
        filter.addEventListener("click", async function () {

            searchIcon.classList.remove("animate-circle");
            searchFilterBlock.style.opacity = '0';


            const classMap = {
                "filterName": "name",
                "filterAge": "age",
                "filterId": "id"
            };

            for (const className in classMap) {
                if (filter.classList.contains(className)) {
                    filterOption = classMap[className];
                    await GetPersons(userSearchInput.value.trim()); 
                    break;
                }
            }

            filterI.forEach(otherFilter => {
                if (otherFilter !== filter) {
                    otherFilter.classList.remove("checkClass");
                }
                else {
                    otherFilter.classList.add("checkClass")
                }
            });

        });
    });
}
FilterBar();

let addUserForm = document.querySelector(".userForm");
addUserForm.addEventListener("submit", function (event) {
    event.preventDefault();

    const userFormData = {
        Name: document.getElementById("name").value.trim(),
        Age: document.getElementById("age").value.trim(),
    };

    SendDataToServer(userFormData);
});
async function SendDataToServer(data) {
    const urlApi = "api/users";
    const methodName = "POST";

    const response = await fetch(urlApi, {
        method: methodName,
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        },
        body: JSON.stringify(data),
    });

    if (!response.ok) throw Error("Server or connection error!");

    const result = await response.json();
    const name = result.name;
    const age = result.age;
    const id = result.id;

    let usersBlocksContainer = document.getElementById("usersContainer")
    let newUserBlock = CreateNewUserBlock(name, age, id);


    let sadSmile = usersContainer.querySelector(".notFoundSmile");


    if (UserIsCorrectToData(newUserBlock) == true)
    {
        if (sadSmile)
            sadSmile.remove();

   
        usersBlocksContainer.appendChild(newUserBlock);
        ScrollToBottom(usersBlocksContainer);
        AddAnimate(newUserBlock);
    }

}
function CreateNewUserBlock(name, age, id) {
    let newUserBlock = document.createElement("div");
    newUserBlock.classList.add("userBlock");

    let nameParagraph = document.createElement("p");
    nameParagraph.classList.add("userName");
    nameParagraph.classList.add("userInfo");
    nameParagraph.textContent = name;

    let idParagraph = document.createElement("p");
    idParagraph.classList.add("userId");
    idParagraph.textContent = id;

    let ageParagraph = document.createElement("p");
    ageParagraph.classList.add("userAge");
    ageParagraph.classList.add("userInfo")
    ageParagraph.textContent = age;

    let deleteButton = document.createElement("button");
    deleteButton.classList.add("modificationButton");
    deleteButton.classList.add("deleteButton");

    let editButton = document.createElement("button");
    editButton.classList.add("modificationButton");
    editButton.classList.add("editButton");

    let deleteImage = document.createElement("img");
    deleteImage.src = "images/deleteIcon.png";
    deleteImage.classList.add("modificationIcon");

    deleteButton.appendChild(deleteImage);

    let editImage = document.createElement("img");
    editImage.src = "images/editUser.png";
    editImage.classList.add("modificationIcon");

    editButton.appendChild(editImage);

    ApplyElementsToBlock(newUserBlock, [nameParagraph, ageParagraph, editButton, deleteButton, idParagraph]);
    SetButtonDelete(deleteButton);
    SetButtonEdit(editButton);

    return newUserBlock;
}
function ApplyElementsToBlock(block, elements) {
    if (!block || Array.isArray(elements) == false) return;

    for (let index = 0; index < elements.length; index++) {
        block.appendChild(elements[index]);
    }
};
function SetButtonDelete(button) {
    button.addEventListener("click", async function () {

        let parentDiv = button.closest("div");
        let userId = parentDiv.querySelector(".userId").textContent;

        let methodName = "DELETE";
        let urlApi = `api/users/${userId}`;

        try {
            const response = await fetch(urlApi, {
                method: methodName,
                headers: {
                    "Content-Type": "application/json",
                },
            });

            if (!response.ok) throw new Error("Failed to delete user!");

            const result = await response.json();
            DeleteAnimate(parentDiv);

            if (CheckUsersQuantityInContainer()) {
                let userContainer = document.getElementById("usersContainer");
                let notFoundSmile = CreateNotFoundSmile();

                if (notFoundSmile !== undefined)
                    userContainer.appendChild(notFoundSmile);
            }
          
        }
        catch (Exception) {
            alert(Exception);
        }

    });
};
function SetButtonEdit(button) {
    button.addEventListener("click", function () {

        let parentUserDiv = button.closest("div");
        let userData = parentUserDiv.querySelectorAll("p");
        let userId = parentUserDiv.querySelector(".userId").textContent;
        let userFeaturesList = [];
        userFeaturesList.push(userId);

        function pWithInput(p, index, userData) {

            if (p.classList.contains("userId") == true) {
                SetNewDataToServer(userFeaturesList, parentUserDiv);
                return;
            }

            let newP = document.createElement("p");
            newP.style.display = 'none';
            newP.classList.add("userInfo");

            let input = document.createElement("input");
            input.classList.add("editInput");

            switch (index) {
                case 0:
                    input.classList.add("editInputName");
                    input.type = "text";
                    input.maxLength = 14;
                    newP.classList.add("userName");
                    break;
                case 1:
                    input.classList.add("editInputAge");
                    input.type = "number";
                    setConstraintForNumberInput(input);
                    newP.classList.add("userAge");
                    break;
            }

            input.value = p.textContent;
            p.replaceWith(input);
            input.focus();

            input.addEventListener("keydown", function (e) {
                if (e.key === "Enter") {

                    newP.textContent = input.value;
                    userFeaturesList.push(input.value);

                    newP.style.display = 'block';
                    input.replaceWith(newP);

                    userData[index] = newP;

                    if (index < userData.length - 1) {
                        pWithInput(userData[index + 1], index + 1, userData);
                    }
                }
            });
        }

        if (userData.length > 0) {
            pWithInput(userData[0], 0, userData);
        }
    });
};
async function SetNewDataToServer(dataMassive, userDiv) {

    const updatedUserData = {
        Id: dataMassive[0],
        Name: dataMassive[1],
        Age: dataMassive[2]
    };

    let urlApi = "api/users";
    let methodName = "PUT";

    const response = await fetch(urlApi, {
        method: methodName,
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(updatedUserData),
    });

    if (!response.ok)
        throw Error("Cannot update user data!");

    const result = await response.json();
    await EditAnimate(userDiv);

    await GetPersons(userSearchInput.value.trim());
};
userSearchInput.addEventListener("input", async function () {

    var searchOutput = document.querySelector(".output-overlay");
    let inputValue = this.value.trim();
    searchOutput.innerHTML = "";

    for (let i = 0; i < inputValue.length; i++) {

        let letter = document.createElement('span');
        letter.textContent = inputValue[i];

        if (i === inputValue.length - 1) {
            letter.classList.add('zoom-effect');
        }

        searchOutput.appendChild(letter);
    }

    await GetPersons(inputValue);
});
async function GetPersons(searchInfo) {

        let urlApi = "api/users";
        let methodName = "GET";

            const response = await fetch(urlApi, {
                method: methodName,
                headers: {
                    'Data-filter': `${filterOption}`,
                    'Accept': 'application/json',
                    'Search-value': `${searchInfo}`,
                    'Order-type': `${orderType}`,
                    'Order-filter': `${orderFilter}`
                }
            });

            if (!response.ok) throw Error("Cannot get users!");

            const resultUsers = await response.json();
            let usersBlockMassive = [];

            resultUsers.forEach(user => {
                let userBlock = CreateNewUserBlock(user.name, user.age, user.id)
                usersBlockMassive.push(userBlock);
            })

            LoadUsers(usersBlockMassive);
}
function LoadUsers(massive) {

    if (!Array.isArray(massive))
        return;

    let usersContainer = document.getElementById("usersContainer");
    usersContainer.querySelectorAll(".userBlock").forEach(element => element.remove());

    if (massive.length == 0) {

        if (!usersContainer.querySelector('.notFoundSmile')) {

            let notFound = document.createElement("img");
            notFound.classList.add('notFoundSmile');
            notFound.src = "images/1739095189632.png";
            usersContainer.appendChild(notFound);

            setTimeout(() => {
                notFound.classList.add('appear');
            }, 10); 

        }
    }
    else {
        if (usersContainer.querySelector('.notFoundSmile'))
            usersContainer.querySelector('.notFoundSmile').remove();
    }


    for (let index = 0; index < massive.length; index++) {
        massive[index].style.opacity = 0;
        usersContainer.appendChild(massive[index]);

        let step = 0;
        let interval = setInterval(() => {
            step += 0.1;
            massive[index].style.opacity = step.toFixed(1);

            if (step >= 0.7) {
                clearInterval(interval);
                massive[index].removeAttribute("style");
            }
        }, 100);
    }
}

// ANIMATIONS
function DeleteAnimate(userBlock) {

    userBlock.style.transition = "height 0.4s ease, opacity 0.4s ease, background-color 0.4s ease";
    userBlock.style.backgroundColor = "red";
    userBlock.style.opacity = "0";
    userBlock.style.height = "0";
    userBlock.style.padding = "0";

    let paragraphs = userBlock.querySelectorAll("p");
    paragraphs.forEach(p => {
        p.style.transition = "transform 0.4s ease, opacity 0.1s ease";
        p.style.transformOrigin = "top"; 
        p.style.transform = "scaleY(0)";
        p.style.opacity = "0";
        p.style.display = "block"; 
    });

    setTimeout(() => {
        userBlock.remove();
    }, 400);
};
function AddAnimate(userBlock) {

    userBlock.style.transform = "scaleX(0.8)";
    userBlock.style.backgroundColor = "lightgreen";

    setTimeout(() => {
        userBlock.removeAttribute("style");
    }, 200);
};
function EditAnimate(userBlock) {

    return new Promise(resolve => {
        userBlock.style.backgroundColor = "deepskyblue";
        userBlock.style.transform = "scale(0.76)";
        userBlock.style.opacity = "0.6";

        setTimeout(() => {

            userBlock.removeAttribute("style");

            setTimeout(() => {
                resolve(); 
            }, 200);

        }, 150);
    });
};
// ANIMATIONS

function UserIsCorrectToData(userBlock) {

    let name = userBlock.querySelector(".userName").textContent.trim().toLowerCase();
    let age = userBlock.querySelector(".userAge").textContent.trim().toLowerCase();
    let id = userBlock.querySelector(".userId").textContent.trim().toLowerCase();

    let compareParametr;

    switch (filterOption) {
        case "name":
            compareParametr = name;
            break;
        case "age":
            compareParametr = age;
            break;
        case "id":
            compareParametr = id;
            break;
    }

    if (compareParametr.startsWith(userSearchInput.value.trim().toLowerCase()) || userSearchInput.value.trim() === "")
        return true;
    else
        return false;
}; //CHECK VALIDATION TO SHOW BLOCK
function ScrollToBottom(container) {
    container.scrollTop = container.scrollHeight;
}; //MOVE BLOCK SCROLL TO DOWN 
function CheckUsersQuantityInContainer() {

    let usersContainer = document.getElementById("usersContainer");
    let users = usersContainer.querySelectorAll(".userBlock");

    return users.length - 1 === 0;
        
}; // CHECK IF USERS QUANTITY EQUALS 0
function CreateNotFoundSmile() {

    let usersContainer = document.getElementById("usersContainer");

    if (!usersContainer.querySelector(".notFoundSmile")) {

        let notFound = document.createElement("img");
        notFound.style.opacity = "0.6";
        notFound.classList.add('notFoundSmile');
        notFound.src = "images/1739095189632.png";
        return notFound;
    }
    
}; // CREATE NOT FOUND IMAGE





//GG
function initializeOrderType() {

    const usersContainer = document.getElementById("usersContainer");

    let orderTypeButton = document.querySelector(".orderType");
    let p = document.createElement("p");
    let orderTypeIndex = 0;
    let types = ["X", "D", "A"];

    let mapDictionary = {
        "X": "none",
        "D": "descending",
        "A": "ascending"
    };

    p.textContent = types[orderTypeIndex];
    p.style.fontSize = "0.6vw";
    p.style.margin = "0";
    p.style.padding = "0";
    p.style.userSelect = "none";
    p.style.fontWeight = "900";

    orderTypeButton.appendChild(p);

    orderTypeButton.addEventListener("click", async function () {
        orderTypeIndex = orderTypeIndex === types.length - 1 ? 0 : orderTypeIndex + 1;

        p.textContent = types[orderTypeIndex];
        orderType = mapDictionary[types[orderTypeIndex]];

        if (orderType === "none") {
            DeletePropertyOrderBlockIfExsitst();
        } else {
            if (!usersContainer.querySelector(".orderProperty"))
                usersContainer.appendChild(CreatePropertyOrderBlock());
        }
        await GetPersons(userSearchInput.value.trim());
    });

    function CreatePropertyOrderBlock() {

        let propOrderBlock = document.createElement("div");

        let letter = document.createElement("span");
        letter.textContent = "A";
        letter.style.pointerEvents = "none";
        letter.style.fontSize = "4.5px";
        letter.style.fontWeight = "bold";
        letter.style.margin = "0";
        letter.style.marginTop = "0.5%";
        letter.style.display = "block";
        letter.style.lineHeight = "1";
        letter.style.fontFamily = "Arial Black, sans-serif";
        letter.style.fontStretch = "expanded";
        propOrderBlock.appendChild(letter);
        propOrderBlock.classList.add("blueHover");

        propOrderBlock.classList.add("orderProperty");

        setTimeout(() => {
            propOrderBlock.classList.add("orderPropertyAnimate");
        }, 100);

        ChangeTheFilterProperty(propOrderBlock);

        return propOrderBlock;
    }
    function DeletePropertyOrderBlockIfExsitst() {
        let orderPropertyBlock = usersContainer.querySelector(".orderProperty");
        if (orderPropertyBlock) {
            orderPropertyBlock.classList.remove("orderPropertyAnimate");

            setTimeout(() => {
                orderPropertyBlock.remove();
                orderFilter = "age";
            }, 300);
        }
    }

    function ChangeTheFilterProperty(block) {

        let states = ["Age", "Name"];
        let stateIndex = 0;
        let string = states[stateIndex].split("");

        block.addEventListener("click", async function () {
            stateIndex = stateIndex === states.length - 1 ? 0 : stateIndex + 1;
            block.innerHTML = "";
            block.classList.add(states[stateIndex] === "NAME" ? "orangeHover" : "blueHover");
            block.classList.remove(states[stateIndex] === "NAME" ? "blueHover" : "orangeHover");
            string = states[stateIndex].split("");

            let letter = document.createElement("span");
            letter.textContent = string[0];
            letter.style.pointerEvents = "none";
            letter.style.fontSize = "4.5px";
            letter.style.fontWeight = "bold";
            letter.style.margin = "0";
            letter.style.marginTop = "0.5%";
            letter.style.display = "block";
            letter.style.lineHeight = "1";
            letter.style.fontFamily = "Arial Black, sans-serif";
            letter.style.fontStretch = "expanded";

            block.appendChild(letter);

            letter.addEventListener("click", function () {
                letter.style.transform = "scale(1.5)";
                setTimeout(() => {
                    letter.removeAttribute("style");
                }, 100);
            });

            block.removeAttribute("style");
            switch (stateIndex) {
                case 0:
                    block.style.backgroundColor = "lightskyblue";
                    orderFilter = "age";
                    break;
                case 1:
                    block.style.backgroundColor = "orange";
                    orderFilter = "name";
                    break;
            }
            await GetPersons(userSearchInput.value.trim());
        });
    }
};
initializeOrderType();