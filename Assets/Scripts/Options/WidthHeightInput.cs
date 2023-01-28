using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WidthHeightInput : MonoBehaviour
{
    [SerializeField] private Button _returnButton;
    [SerializeField] private TMP_InputField _widthInput;
    [SerializeField] private TMP_InputField _heightInput;
    [SerializeField] private ErrorListComponent _errors;
    private int _minWidth = 5, _minHeight = 5;
    private int _maxWidth = 500, _maxHeight = 500;
    public List<string> ValidationErrors
    {
        get
        {
            var errors = new List<string>();
            if (Width < _minWidth)
                errors.Add($"Szerokoœæ planszy jest zbyt ma³a ({_minWidth} jest minimaln¹ szerokoœci¹).");
            else if (Width > _maxWidth)
                errors.Add($"Szerokoœæ planszy jest zbyt du¿a ({_maxWidth} jest maksymaln¹ szerokoœci¹).");
            if (Height < _minHeight)
                errors.Add($"Wysokoœæ planszy jest zbyt ma³a ({_minHeight} jest minimaln¹ wysokoœci¹).");
            else if (Height > _maxWidth)
                errors.Add($"Wysokoœæ planszy jest zbyt du¿a ({_maxHeight} jest maksymaln¹ wysokoœci¹).");
            return errors;
        }
    }
    private int Width
    {
        get => _width;
        set
        {
            _width = value;
            Validate();
        }
    }
    private int Height
    {
        get => _height;
        set
        {
            _height = value;
            Validate();
        }
    }

    void Awake()
    {
        _widthInput.text = Width.ToString();
        _widthInput.onValueChanged.AddListener(s => Width = int.Parse(s));

        _heightInput.text = Height.ToString();
        _heightInput.onValueChanged.AddListener(s => Height = int.Parse(s));

        _returnButton.onClick.AddListener(ReturnToMenu);
        Validate();
    }
    private void Validate()
    {
        var errors = ValidationErrors;
        var isValid = !errors.Any();
        _returnButton.interactable = isValid;

        if (isValid == _errors.gameObject.activeInHierarchy)
            _errors.gameObject.SetActive(!isValid);
        _errors.tipToShow = string.Join("\n", errors);

    }

    private void ReturnToMenu()
    {
        DataHolder.Instance.BoardWidth = Width;
        DataHolder.Instance.BoardHeight = Height;
        SceneManager.LoadScene("MainMenu");
    }

    private int _width = DataHolder.Instance.BoardWidth;
    private int _height = DataHolder.Instance.BoardHeight;
}
