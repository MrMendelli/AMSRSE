﻿using AMSRSE.DataViewer.DataModels.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AMSRSE.DataViewer.DataModels
{
    public abstract class EditableModel : DependencyObject, IDataErrorInfo, ISupportInitialize
    {
        #region Dependency Properties

        public static readonly DependencyPropertyKey HasChangesPropertyKey =
            DependencyProperty.RegisterReadOnly("HasChanges", typeof(bool), typeof(EditableModel), null);

        public static readonly DependencyProperty HasChangesProperty =
            HasChangesPropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey IsValidPropertyKey =
            DependencyProperty.RegisterReadOnly("IsValid", typeof(bool), typeof(EditableModel), null);

        public static readonly DependencyProperty IsValidProperty =
            IsValidPropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey ValidationErrorsPropertyKey =
            DependencyProperty.RegisterReadOnly("ValidationErrors", typeof(IDictionary), typeof(EditableModel), null);

        public static readonly DependencyProperty ValidationErrorsProperty =
            ValidationErrorsPropertyKey.DependencyProperty;

        #endregion Dependency Properties

        #region Properties

        public bool HasChanges
        {
            get { return (bool)GetValue(HasChangesProperty); }
            protected set { SetValue(HasChangesPropertyKey, value); }
        }

        public bool IsValid
        {
            //get { return ModelValidation.GetModelValidation(this).IsValid; }
            get { return (bool)GetValue(IsValidProperty); }
            private set { SetValue(IsValidPropertyKey, value); }
        }

        public IDictionary ValidationErrors
        {
            get { return (IDictionary)GetValue(ValidationErrorsProperty); }
            private set { SetValue(ValidationErrorsPropertyKey, value); }
        }

        #endregion Properties

        #region Members

        protected Dictionary<DependencyProperty, object> _originalPropertyValues;

        #endregion Members

        #region Events

        public delegate void ModelPropertyChangedHandler(DependencyProperty p, object newValue);
        public event ModelPropertyChangedHandler ModelPropertyChanged;

        #endregion Events

        #region Ctor

        public EditableModel()
        {
            //ModelValidation.SetModelValidation(this, new ModelValidation());
            ValidationErrors = new Dictionary<string, string>();
            _originalPropertyValues = new Dictionary<DependencyProperty, object>();
        }

        

        #endregion Ctor

        #region IDataErrorInfo

        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                //var modelValidation = ModelValidation.GetModelValidation(this);
                //var validationResult = modelValidation.ValidateProperty(this, propertyName);
                //modelValidation.Validate(this);

                //return validationResult;

                string error = string.Empty;
                var dpDescriptor = DependencyPropertyDescriptor.FromName(propertyName, this.GetType(), this.GetType());

                if (dpDescriptor != null)
                {
                    var results = new List<ValidationResult>(1);

                    var result = Validator.TryValidateProperty(
                        value: this.GetValue(dpDescriptor.DependencyProperty),
                        validationContext: new ValidationContext(this)
                        {
                            MemberName = propertyName
                        },
                        validationResults: results);

                    if (!result)
                    {
                        var validationResult = results.First();
                        error = validationResult.ErrorMessage;

                        if (ValidationErrors.Contains(propertyName))
                            ValidationErrors[propertyName] = error;

                        else
                            ValidationErrors.Add(propertyName, error);
                    }

                    else
                    {
                        if (ValidationErrors.Contains(propertyName))
                            ValidationErrors.Remove(propertyName);
                    }
                }

                IsValid = ValidationErrors.Count > 0 ? false : true;
                return error;
            }
        }

        #endregion IDataErrorInfo

        #region ISupportInitialize

        public void BeginInit()
        {
            Test();
        }

        public void EndInit()
        {
            Test();
        }

        #endregion ISupportInitialize

        #region Methods

        protected static DependencyProperty RegisterTracked(string name, Type propertyType, Type ownerType)
        {
            return RegisterTracked(name, propertyType, ownerType, null, null);
        }

        protected static DependencyProperty RegisterTracked(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata)
        {
            return RegisterTracked(name, propertyType, ownerType, typeMetadata, null);
        }

        protected static DependencyProperty RegisterTracked(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata, ValidateValueCallback validateValueCallback)
        {
            object defaultValue;

            if (typeMetadata != null)
                defaultValue = typeMetadata.DefaultValue;

            else
                defaultValue = propertyType.IsValueType ?
                    Activator.CreateInstance(propertyType) :
                    null;

            PropertyMetadata metadata = new PropertyMetadata(
                defaultValue: defaultValue,
                propertyChangedCallback: (d, e) =>
                {
                    CheckDependencyPropertyChanges(d, e);
                    typeMetadata?.PropertyChangedCallback?.Invoke(d, e);
                },
                coerceValueCallback: typeMetadata?.CoerceValueCallback);

            return DependencyProperty.Register(name, propertyType, ownerType, metadata, validateValueCallback);
        }

        protected static DependencyPropertyKey RegisterReadOnlyTracked(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata)
        {
            return RegisterReadOnlyTracked(name, propertyType, ownerType, typeMetadata, null);
        }

        protected static DependencyPropertyKey RegisterReadOnlyTracked(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata, ValidateValueCallback validateValueCallback)
        {
            object defaultValue;

            if (typeMetadata != null)
                defaultValue = typeMetadata.DefaultValue;

            else
                defaultValue = propertyType.IsValueType ?
                    Activator.CreateInstance(propertyType) :
                    null;

            PropertyMetadata metadata = new PropertyMetadata(
                defaultValue: defaultValue,
                propertyChangedCallback: (d, e) =>
                {
                    CheckDependencyPropertyChanges(d, e);
                    typeMetadata?.PropertyChangedCallback?.Invoke(d, e);
                },
                coerceValueCallback: typeMetadata?.CoerceValueCallback);

            return DependencyProperty.RegisterReadOnly(name, propertyType, ownerType, metadata, validateValueCallback);
        }

        //TODO: Try to find a way to call this for dependency properties that have a default value.
        //      Currently, the default value does not get added to the original values dictionary
        //      because the property changed callback never gets fired.
        private static void CheckDependencyPropertyChanges(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditableModel editableModel &&
                !DesignerProperties.GetIsInDesignMode(editableModel))
            {
                if (editableModel._originalPropertyValues.ContainsKey(e.Property))
                {
                    if (!editableModel.HasChanges && editableModel._originalPropertyValues[e.Property] != e.NewValue)
                        editableModel.HasChanges = true;
                }

                else
                    editableModel._originalPropertyValues.Add(e.Property, e.NewValue);

                editableModel.RaiseModelPropertyChanged(e.Property, e.NewValue);
            }
        }

        protected void RaiseModelPropertyChanged(DependencyProperty p, object newValue)
        {
            ModelPropertyChanged?.Invoke(p, newValue);
        }

        public void RevertChanges()
        {
            for (int p = 0; p < _originalPropertyValues.Count; p++)
                SetValue(_originalPropertyValues.ElementAt(p).Key, _originalPropertyValues.ElementAt(p).Value);

            OnRevertChanges();

            HasChanges = false;
        }

        protected virtual void OnRevertChanges()
        {

        }

        protected virtual void Test()
        {

        }

        #endregion Methods
    }
}
