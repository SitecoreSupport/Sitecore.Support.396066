using Sitecore.Data.Items;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core;
using Sitecore.Modules.EmailCampaign.Speak.Web.Core.Repositories;
using Sitecore.Modules.EmailCampaign.Speak.Web.UI.WebControls;
using Sitecore.Speak.Web.UI.WebControls;
using Sitecore.Web.UI.WebControls;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web.UI;

namespace Sitecore.Support.Modules.EmailCampaign.Speak.Web.UI.WebControls
{
    public class AbnVariant : FieldEditor<System.Web.UI.WebControls.ObjectDataSource>
    {
        // Fields
        private MessageItem messageItem;
        private NameValueCollection parameters;

        // Methods
        public AbnVariant() : base(new System.Web.UI.WebControls.ObjectDataSource())
        {
        }

        private void AbnVariantUpdating(object sender, System.Web.UI.WebControls.ObjectDataSourceMethodEventArgs e)
        {
            MessageTestVariant variant = e.InputParameters[0] as MessageTestVariant;
            if (variant != null)
            {
                variant.MessageId = this.Page.Request.QueryString["id"];
                variant.VariantId = this.Params["ItemId"];
                if ((this.MessageItem != null) && (this.MessageItem is HtmlMailBase))
                {
                    Item innerItem = this.MessageItem.InnerItem;
                    Item item2 = innerItem;
                    if (innerItem.GetChildren().Count > 0)
                    {
                        item2 = (from i in innerItem.GetChildren()
                                 where i.Fields["AlternateText"] != null
                                 select i).FirstOrDefault<Item>();
                    }
                    if ((item2.Fields["AlternateText"] != null) && (item2["AlternateText"] != variant.AlternateText))
                    {
                        item2.Editing.BeginEdit();
                        item2["AlternateText"] = variant.AlternateText;
                        item2.Editing.EndEdit();
                    }
                }
            }
        }

        protected override void AddFieldToFieldEditor(FieldEditor fieldEditor, Item item)
        {
            base.AddFieldToFieldEditor(fieldEditor, item);
            DataField field = fieldEditor.Fields[fieldEditor.Fields.Count - 1];
            if ((field.Name == "AlternateText") && !(this.MessageItem is HtmlMailBase))
            {
                fieldEditor.Fields.Remove(field);
            }
        }

        protected override DataField GetField(Item item)
        {
            DataField field = base.GetField(item);
            if (this.IsSubjectFieldForWebPageMail(item))
            {
                TemplateField field2 = field as TemplateField;
                if (field2 != null)
                {
                    field2.ReadTemplate = field2.EditTemplate;
                }
            }
            if (item.Name != "Body")
            {
                return field;
            }
            TemplateField field3 = field as TemplateField;
            if (field3 == null)
            {
                return field;
            }
            AbnFieldRenderer renderer = new AbnFieldRenderer
            {
                Readonly = true
            };
            field3.ReadTemplate = renderer;
            return field3;
        }

        protected override ITemplate GetFieldRendering(Item item)
        {
            if (this.IsSubjectFieldForWebPageMail(item))
            {
                return new SubjectFieldRenderer();
            }
            return base.GetFieldRendering(item);
        }

        protected override void InitializeDataSourceControl()
        {
            base.InitializeDataSourceControl();
            base.dataSourceControl.TypeName = typeof(MessageTestVariantRepository).FullName;
            base.dataSourceControl.DataObjectTypeName = typeof(MessageTestVariant).FullName;
            base.dataSourceControl.Updating += new System.Web.UI.WebControls.ObjectDataSourceMethodEventHandler(this.AbnVariantUpdating);
            base.dataSourceControl.SelectMethod = "Get";
            base.dataSourceControl.UpdateMethod = "Update";
            base.dataSourceControl.SelectParameters.Clear();
            System.Web.UI.WebControls.Parameter parameter = new System.Web.UI.WebControls.Parameter("variantID")
            {
                DefaultValue = this.Params["ItemId"]
            };
            base.dataSourceControl.SelectParameters.Add(parameter);
            System.Web.UI.WebControls.Parameter parameter2 = new System.Web.UI.WebControls.Parameter("messageID")
            {
                DefaultValue = this.Page.Request.QueryString["id"]
            };
            base.dataSourceControl.SelectParameters.Add(parameter2);
        }

        protected override void InitializeFieldEditors()
        {
            base.fieldEditorLeft.DataSourceID = base.dataSourceControl.ID;
            base.fieldEditorRight.DataSourceID = base.dataSourceControl.ID;
            base.InitializeFieldEditors();
        }

        private bool IsSubjectFieldForWebPageMail(Item item) =>
            ((item["Name"] == "Subject") && (this.MessageItem.GetType() == typeof(WebPageMail)));

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ID = "AbnVariant";
        }

        // Properties
        public MessageItem MessageItem =>
            (this.messageItem ?? (this.messageItem = UIFactory.Instance.GetSpeakContext().Message));

        public virtual NameValueCollection Params
        {
            get
            {
                if (this.parameters != null)
                {
                    return this.parameters;
                }
                return (this.parameters ?? (this.parameters = StringUtil.ParseNameValueCollection(this.Parameters, '&', '=')));
            }
        }
    }


}