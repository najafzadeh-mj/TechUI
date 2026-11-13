using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using TechUI.Mvc.Chart;
using TechUI.Mvc.Captcha;
using TechUI.Mvc.Chat;
using TechUI.Mvc.DropDownList;
using TechUI.Mvc.DropDownTree;
using TechUI.Mvc.FileUpload;
using TechUI.Mvc.Grid;
using TechUI.Mvc.Inputs;
using TechUI.Mvc.Layout;
using TechUI.Mvc.MultiSelect;
using TechUI.Mvc.Timeline;
using TechUI.Mvc.TreeView;
using TechUI.Mvc.Wizard;

namespace TechUI.Mvc.Html
{
    /// <summary>
    /// Root fluent builder for all TechUI components.
    /// </summary>
    public sealed class TechBuilder
    {
        internal IHtmlHelper Html => _html;

        private readonly IHtmlHelper _html;

        public TechBuilder(IHtmlHelper html)
        {
            _html = html;
        }

        public GridBuilder<TModel> Grid<TModel>()
        {
            return new GridBuilder<TModel>(_html);
        }

        public ChartBuilder Chart(string? id = null)
        {
            return new ChartBuilder(id);
        }

        public DropdownListBuilder DropDownList(string name)
        {
            return new DropdownListBuilder(_html).Name(name);
        }

        public DropdownListBuilder DropDownListFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new DropdownListBuilder(_html).Name(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        public DropDownTreeBuilder DropDownTree(string name, string? id = null)
        {
            return new DropDownTreeBuilder(name, id);
        }

        public DropDownTreeBuilder DropDownTreeFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new DropDownTreeBuilder(name);

            if (typeof(TValue) != typeof(string) &&
                typeof(System.Collections.IEnumerable).IsAssignableFrom(typeof(TValue)))
            {
                var (_, values) = ResolveValuesFor(expression);

                builder.Selection("multiple").Values(values);
            }
            else if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        public TextBoxBuilder TextBox(string name)
        {
            return new TextBoxBuilder(name);
        }

        public TextBoxBuilder TextBoxFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new TextBoxBuilder(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        public EditorBuilder Editor(string name)
        {
            return new EditorBuilder(name);
        }

        public EditorBuilder EditorFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new EditorBuilder(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        public TextAreaBuilder TextArea(string name)
        {
            return new TextAreaBuilder(name);
        }

        public TextAreaBuilder TextAreaFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new TextAreaBuilder(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        public NumericTextBoxBuilder NumericTextBox(string name)
        {
            return new NumericTextBoxBuilder(name);
        }

        public NumericTextBoxBuilder NumericTextBoxFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new NumericTextBoxBuilder(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        public CheckBoxBuilder CheckBox(string name)
        {
            return new CheckBoxBuilder(name);
        }

        public CheckBoxBuilder CheckBoxFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new CheckBoxBuilder(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        public RadioButtonBuilder RadioButton(string name)
        {
            return new RadioButtonBuilder(name);
        }

        public RadioButtonBuilder RadioButtonFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new RadioButtonBuilder(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        public TimePickerBuilder TimePicker(string name)
        {
            return new TimePickerBuilder(name);
        }

        public TimePickerBuilder TimePickerFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new TimePickerBuilder(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        public DatePickerBuilder DatePicker(string name)
        {
            return new DatePickerBuilder(name);
        }

        public DatePickerBuilder DatePickerFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new DatePickerBuilder(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        public DateRangePickerBuilder DateRange(
            string startName,
            string endName)
        {
            return new DateRangePickerBuilder(startName, endName);
        }

        public DateRangePickerBuilder DateRangeFor<TModel, TStart, TEnd>(
            Expression<Func<TModel, TStart>> startExpression,
            Expression<Func<TModel, TEnd>> endExpression)
        {
            ArgumentNullException.ThrowIfNull(startExpression);
            ArgumentNullException.ThrowIfNull(endExpression);

            var (startName, startValue, startMetadata) =
                ResolveFor(startExpression);

            var (endName, endValue, _) = ResolveFor(endExpression);

            var builder = new DateRangePickerBuilder(startName, endName);

            if (!string.IsNullOrEmpty(startValue))
            {
                builder.Start(startValue);
            }

            if (!string.IsNullOrEmpty(endValue))
            {
                builder.End(endValue);
            }

            ApplyValidation(builder, startMetadata, startName);

            return builder;
        }

        public MaskedTextBoxBuilder MaskedTextBox(string name)
        {
            return new MaskedTextBoxBuilder(name);
        }

        public TabsBuilder Tabs(string? id = null)
        {
            return new TabsBuilder(id);
        }

        public AccordionBuilder Accordion(string? id = null)
        {
            return new AccordionBuilder(id);
        }

        public TreeViewBuilder TreeView(string? id = null)
        {
            return new TreeViewBuilder(id);
        }

        public SpinnerBuilder Spinner()
        {
            return new SpinnerBuilder();
        }

        public MultiSelectBuilder MultiSelect(string name)
        {
            return new MultiSelectBuilder(_html, name);
        }

        public FileUploadBuilder FileUpload(string name)
        {
            return new FileUploadBuilder(_html, name);
        }

        public CaptchaBuilder Captcha(string name)
        {
            return new CaptchaBuilder(_html, name);
        }

        public WizardBuilder Wizard(string? id = null)
        {
            return new WizardBuilder(id);
        }

        public TimelineBuilder Timeline(string? id = null)
        {
            return new TimelineBuilder(id);
        }

        public BreadcrumbBuilder Breadcrumb(string? id = null)
        {
            return new BreadcrumbBuilder(id);
        }

        public ChatBuilder Chat(string? id = null)
        {
            return new ChatBuilder(id);
        }

        public MultiSelectBuilder MultiSelectFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, values) = ResolveValuesFor(expression);

            return new MultiSelectBuilder(_html, name).Values(values);
        }

        public MaskedTextBoxBuilder MaskedTextBoxFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            var (name, value, metadata) = ResolveFor(expression);

            var builder = new MaskedTextBoxBuilder(name);

            if (!string.IsNullOrEmpty(value))
            {
                builder.Value(value);
            }

            ApplyValidation(builder, metadata, name);

            return builder;
        }

        private (string Name, string? Value, ModelMetadata? Metadata) ResolveFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            var modelExpression = CreateModelExpression(expression);

            var expressionText = GetExpressionText(expression);

            var name = modelExpression?.Name
                ?? _html.ViewData.TemplateInfo.GetFullHtmlFieldName(expressionText);

            string? value = null;

            if (_html.ViewData.ModelState.TryGetValue(name, out var entry) &&
                entry.AttemptedValue is not null)
            {
                value = entry.AttemptedValue;
            }
            else if (modelExpression?.Model is { } modelValue)
            {
                value = Convert.ToString(modelValue, CultureInfo.InvariantCulture);
            }

            return (name, value, modelExpression?.Metadata);
        }

        private (string Name, List<string> Values) ResolveValuesFor<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            var modelExpression = CreateModelExpression(expression);

            var expressionText = GetExpressionText(expression);

            var name = modelExpression?.Name
                ?? _html.ViewData.TemplateInfo.GetFullHtmlFieldName(expressionText);

            var values = new List<string>();

            if (_html.ViewData.ModelState.TryGetValue(name, out var entry))
            {
                if (entry.RawValue is string[] raw)
                {
                    values.AddRange(raw.Where(item => !string.IsNullOrEmpty(item)));
                }
                else if (!string.IsNullOrEmpty(entry.AttemptedValue))
                {
                    values.AddRange(entry.AttemptedValue
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(item => item.Trim())
                        .Where(item => item.Length > 0));
                }
            }
            else if (modelExpression?.Model is System.Collections.IEnumerable items &&
                modelExpression.Model is not string)
            {
                foreach (var item in items)
                {
                    if (item is null)
                    {
                        continue;
                    }

                    var text = Convert.ToString(item, CultureInfo.InvariantCulture);

                    if (!string.IsNullOrEmpty(text))
                    {
                        values.Add(text);
                    }
                }
            }

            return (name, values);
        }

        private ModelExpression? CreateModelExpression<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression)
        {
            try
            {
                var viewData = _html.ViewData as ViewDataDictionary<TModel>
                    ?? new ViewDataDictionary<TModel>(_html.ViewData);

                var provider = _html.ViewContext?.HttpContext.RequestServices
                    .GetService<IModelExpressionProvider>();

                return provider?
                    .CreateModelExpression(viewData, expression);
            }
            catch (InvalidCastException)
            {
                // The current view model is not TModel.
            }
            catch (InvalidOperationException)
            {
                // Metadata is unavailable; fall back to the expression text.
            }

            return null;
        }

        private void ApplyValidation(
            IValidationBuilder builder,
            ModelMetadata? metadata,
            string name)
        {
            var attributes = metadata is null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : BuildValidationData(metadata);

            // A non-nullable bool is implicitly "required" in MVC metadata,
            // but an unchecked checkbox posts the hidden "false" value and is valid.
            if (builder is CheckBoxBuilder)
            {
                attributes.Remove("data-val-required");
            }

            var error = GetModelError(name, metadata);

            if (attributes.Count == 0 && string.IsNullOrEmpty(error))
            {
                return;
            }

            builder.ApplyValidation(attributes, error);
        }

        private string? GetModelError(string name, ModelMetadata? metadata)
        {
            if (_html.ViewData.ModelState.TryGetValue(name, out var entry) &&
                entry.Errors.Count > 0)
            {
                var message = entry.Errors[0].ErrorMessage;

                if (string.IsNullOrWhiteSpace(message))
                {
                    return null;
                }

                var displayName = string.IsNullOrWhiteSpace(metadata?.DisplayName)
                    ? metadata?.Name ?? name
                    : metadata.DisplayName;

                return LocalizeDefaultError(message, displayName) ?? message;
            }

            return null;
        }

        private static string? LocalizeDefaultError(
            string message,
            string displayName)
        {
            var match = Regex.Match(
                message, @"^The (.+) field is required\.$");

            if (match.Success)
            {
                return $"وارد کردن {displayName} الزامی است.";
            }

            match = Regex.Match(
                message,
                @"^The field (.+) must be between (.+) and (.+)\.$");

            if (match.Success)
            {
                return $"مقدار {displayName} باید بین " +
                    $"{match.Groups[2].Value} و {match.Groups[3].Value} باشد.";
            }

            match = Regex.Match(
                message,
                @"^The field (.+) must be a string with a minimum length of (\d+) and a maximum length of (\d+)\.$");

            if (match.Success)
            {
                return $"طول {displayName} باید بین " +
                    $"{match.Groups[2].Value} و {match.Groups[3].Value} کاراکتر باشد.";
            }

            match = Regex.Match(
                message,
                @"^The field (.+) must be a string or array type with a minimum length of '(\d+)'\.$");

            if (match.Success)
            {
                return $"طول {displayName} باید حداقل " +
                    $"{match.Groups[2].Value} کاراکتر باشد.";
            }

            match = Regex.Match(
                message,
                @"^The field (.+) must be a string or array type with a maximum length of '(\d+)'\.$");

            if (match.Success)
            {
                return $"طول {displayName} نباید بیشتر از " +
                    $"{match.Groups[2].Value} کاراکتر باشد.";
            }

            if (Regex.IsMatch(
                message, @"^The (.+) field is not a valid e-mail address\.$"))
            {
                return $"فرمت {displayName} معتبر نیست.";
            }

            if (Regex.IsMatch(
                    message, @"^The (.+) field is not a valid phone number\.$") ||
                Regex.IsMatch(
                    message,
                    @"^The (.+) field is not a valid fully-qualified http, https, or ftp URL\.$"))
            {
                return $"فرمت {displayName} معتبر نیست.";
            }

            if (Regex.IsMatch(
                message,
                @"^The (.+) field is not a valid credit card number\.$"))
            {
                return $"شماره کارت {displayName} معتبر نیست.";
            }

            match = Regex.Match(
                message,
                @"^The field (.+) must match the regular expression '(.+)'\.$");

            if (match.Success)
            {
                return $"فرمت {displayName} معتبر نیست.";
            }

            match = Regex.Match(
                message, @"^'(.+)' and '(.+)' do not match\.$");

            if (match.Success)
            {
                return $"{displayName} با {match.Groups[2].Value} مطابقت ندارد.";
            }

            return null;
        }

        private static Dictionary<string, string> BuildValidationData(
            ModelMetadata metadata)
        {
            var data = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            var hasRequired = false;

            foreach (var validator in metadata.ValidatorMetadata)
            {
                switch (validator)
                {
                    case RequiredAttribute required:
                        hasRequired = true;
                        data["data-val-required"] = GetMessage(
                            required.ErrorMessage,
                            "وارد کردن {0} الزامی است.",
                            "The {0} field is required.");
                        break;

                    case StringLengthAttribute length:
                        data["data-val-length"] = GetMessage(
                            length.ErrorMessage,
                            "طول {0} باید بین {2} و {1} کاراکتر باشد.",
                            "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.");
                        data["data-val-length-max"] = length.MaximumLength
                            .ToString(CultureInfo.InvariantCulture);

                        if (length.MinimumLength > 0)
                        {
                            data["data-val-length-min"] = length.MinimumLength
                                .ToString(CultureInfo.InvariantCulture);
                        }

                        break;

                    case MinLengthAttribute minLength:
                        data["data-val-minlength"] = GetMessage(
                            minLength.ErrorMessage,
                            "طول {0} باید حداقل {1} کاراکتر باشد.",
                            "The field {0} must be a string or array type with a minimum length of '{1}'.");
                        data["data-val-minlength-min"] = minLength.Length
                            .ToString(CultureInfo.InvariantCulture);
                        break;

                    case MaxLengthAttribute maxLength:
                        data["data-val-maxlength"] = GetMessage(
                            maxLength.ErrorMessage,
                            "طول {0} نباید بیشتر از {1} کاراکتر باشد.",
                            "The field {0} must be a string or array type with a maximum length of '{1}'.");
                        data["data-val-maxlength-max"] = maxLength.Length
                            .ToString(CultureInfo.InvariantCulture);
                        break;

                    case RangeAttribute range:
                        data["data-val-range"] = GetMessage(
                            range.ErrorMessage,
                            "مقدار {0} باید بین {1} و {2} باشد.",
                            "The field {0} must be between {1} and {2}.");
                        data["data-val-range-min"] = Convert.ToString(
                            range.Minimum, CultureInfo.InvariantCulture) ?? string.Empty;
                        data["data-val-range-max"] = Convert.ToString(
                            range.Maximum, CultureInfo.InvariantCulture) ?? string.Empty;
                        break;

                    case EmailAddressAttribute email:
                        data["data-val-email"] = GetMessage(
                            email.ErrorMessage,
                            "فرمت {0} معتبر نیست.",
                            "The {0} field is not a valid e-mail address.");
                        break;

                    case PhoneAttribute phone:
                        data["data-val-phone"] = GetMessage(
                            phone.ErrorMessage,
                            "فرمت {0} معتبر نیست.",
                            "The {0} field is not a valid phone number.");
                        break;

                    case UrlAttribute url:
                        data["data-val-url"] = GetMessage(
                            url.ErrorMessage,
                            "فرمت {0} معتبر نیست.",
                            "The {0} field is not a valid fully-qualified http, https, or ftp URL.");
                        break;

                    case CreditCardAttribute card:
                        data["data-val-creditcard"] = GetMessage(
                            card.ErrorMessage,
                            "شماره کارت {0} معتبر نیست.",
                            "The {0} field is not a valid credit card number.");
                        break;

                    case RegularExpressionAttribute regex:
                        data["data-val-regex"] = GetMessage(
                            regex.ErrorMessage,
                            "فرمت {0} معتبر نیست.",
                            "The field {0} must match the regular expression '{1}'.");
                        data["data-val-regex-pattern"] = regex.Pattern;
                        break;

                    case CompareAttribute compare:
                        data["data-val-equalto"] = GetMessage(
                            compare.ErrorMessage,
                            "{0} با {1} مطابقت ندارد.",
                            "'{0}' and '{1}' do not match.");
                        data["data-val-equalto-other"] = compare.OtherProperty;
                        break;
                }
            }

            if (!hasRequired && metadata.IsRequired)
            {
                data["data-val-required"] = "وارد کردن {0} الزامی است.";
            }

            if (data.Count > 0)
            {
                data["data-val-name"] = string.IsNullOrWhiteSpace(metadata.DisplayName)
                    ? metadata.Name ?? string.Empty
                    : metadata.DisplayName;
            }

            return data;
        }

        private static string GetMessage(
            string? configured,
            string fallback,
            params string[] frameworkDefaults)
        {
            if (string.IsNullOrWhiteSpace(configured))
            {
                return fallback;
            }

            return frameworkDefaults.Any(item =>
                string.Equals(item, configured, StringComparison.Ordinal))
                ? fallback
                : configured;
        }

        private static string GetExpressionText(LambdaExpression expression)
        {
            var body = expression.Body;

            if (body is UnaryExpression unary)
            {
                body = unary.Operand;
            }

            var parts = new List<string>();

            while (body is MemberExpression member)
            {
                parts.Insert(0, member.Member.Name);
                body = member.Expression;
            }

            return parts.Count > 0
                ? string.Join(".", parts)
                : expression.ToString();
        }

        public string Version()
        {
            return "TechUI 1.0.0";
        }
    }
}
