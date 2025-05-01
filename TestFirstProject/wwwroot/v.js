// CONSTRAINTS FOR NAME AND AGE INPUT
let formAgeInput = document.querySelector(".ageInput");
let formNameInput = document.querySelector(".nameInput");

setConstraintForNumberInput(formAgeInput);
setConstraintForNameInput(formNameInput);

function setConstraintForNumberInput(input) {
    input.addEventListener("input", function () {
        if (input.value.length > 3) {
            input.value = input.value.slice(0, 3);
        }
        if (Number(input.value) > 100)
            input.value = "";
    });
};
function setConstraintForNameInput(input) {
    input.addEventListener("input", function () {
        if (input.value.trim() == "")
            input.setCustomValidity("Name cannot be empty!");
        else
            input.setCustomValidity("");
    })
};
//CONSTRAINTS FOR NAME AND AGE INPUT


// ANIMATION ON THIS INPUTS
function SetUserFormInputsName() {

    let nameAddOutput = document.querySelector('.name-output');
    let nameAddInput = document.querySelector('.nameInput');

    nameAddInput.addEventListener('input', function () {
        let name = nameAddInput.value.trim();
        nameAddOutput.innerHTML = "";

        for (let index = 0; index < name.length; index++) {

            let letter = document.createElement('span');
            letter.textContent = name[index];

            if (index === name.length - 1) {
                letter.classList.add('zoom-effect'); 
            }

            nameAddOutput.appendChild(letter);
        }
    });
};
function SetUserFormInputsAge() {

    let ageAddOutput = document.querySelector('.age-output');
    let ageAddInput = document.querySelector('.ageInput');

    ageAddInput.addEventListener('input', function () {
        let name = ageAddInput.value.trim();
        ageAddOutput.innerHTML = ""; 

        for (let index = 0; index < name.length; index++) {
            let letter = document.createElement('span');
            letter.textContent = name[index];

            if (index === name.length - 1) {
                letter.classList.add('zoom-effect'); 
            }

            ageAddOutput.appendChild(letter);
        }
    });
};

SetUserFormInputsAge();
SetUserFormInputsName();
// ANIMATION ON THIS INPUTS

let resetButton = document.querySelector(".buttonR"); // CLEAR THE OUTPUT SPANS
let addButton = document.querySelector(".buttonA"); // CLEAR THE OUTPUT SPANS AFTER SUBMIT

resetButton.addEventListener('click', function () {
    let nameAddOutput = document.querySelector('.name-output');
    let ageAddOutput = document.querySelector('.age-output');
    nameAddOutput.innerHTML = "";
    ageAddOutput.innerHTML = "";
}); // CLEAR THE OUTPUT SPANS
addButton.addEventListener('click', function () {
    let nameAddOutput = document.querySelector('.name-output');
    let ageAddOutput = document.querySelector('.age-output');
    nameAddOutput.innerHTML = "";
    ageAddOutput.innerHTML = "";

    setTimeout(() => {
        formAgeInput.value = "";
        formNameInput.value = "";
    }, 25);
  
}); // CLEAR THE OUTPUT SPANS AFTER SUBMIT