﻿using System.Linq;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Serialization;
using coreModel = VirtoCommerce.Domain.Marketing.Model;
using webModel = VirtoCommerce.MarketingModule.Web.Model;

namespace VirtoCommerce.MarketingModule.Web.Converters
{
    public static class DynamicContentPublicationConverter
    {
        public static webModel.DynamicContentPublication ToWebModel(this coreModel.DynamicContentPublication publication, ConditionExpressionTree etalonEpressionTree = null)
        {
            var retVal = new webModel.DynamicContentPublication();
            retVal.InjectFrom(publication);
            if (publication.ContentItems != null)
            {
                retVal.ContentItems = publication.ContentItems.Select(x => x.ToWebModel()).ToList();
            }
            if (publication.ContentPlaces != null)
            {
                retVal.ContentPlaces = publication.ContentPlaces.Select(x => x.ToWebModel()).ToList();
            }

            retVal.DynamicExpression = etalonEpressionTree;
            if (!string.IsNullOrEmpty(publication.PredicateVisualTreeSerialized))
            {
                retVal.DynamicExpression = JsonConvert.DeserializeObject<ConditionExpressionTree>(publication.PredicateVisualTreeSerialized);
                if (etalonEpressionTree != null)
                {
                    //Copy available elements from etalon because they not persisted
                    var sourceBlocks = ((DynamicExpression)etalonEpressionTree).Traverse(x => x.Children);
                    var targetBlocks = ((DynamicExpression)retVal.DynamicExpression).Traverse(x => x.Children).ToList();
                    foreach (var sourceBlock in sourceBlocks)
                    {
                        foreach (var targetBlock in targetBlocks.Where(x => x.Id == sourceBlock.Id))
                        {
                            targetBlock.AvailableChildren = sourceBlock.AvailableChildren;
                        }
                    }
                    //copy available elements from etalon
                    retVal.DynamicExpression.AvailableChildren = etalonEpressionTree.AvailableChildren;
                }
            }
            return retVal;
        }

        public static coreModel.DynamicContentPublication ToCoreModel(this webModel.DynamicContentPublication publication, IExpressionSerializer expressionSerializer)
        {
            var retVal = new coreModel.DynamicContentPublication();
            retVal.InjectFrom(publication);
            if (publication.ContentItems != null)
            {
                retVal.ContentItems = publication.ContentItems.Select(x => x.ToCoreModel()).ToList();
            }
            if (publication.ContentPlaces != null)
            {
                retVal.ContentPlaces = publication.ContentPlaces.Select(x => x.ToCoreModel()).ToList();
            }

            if (publication.DynamicExpression != null)
            {
                var conditionExpression = publication.DynamicExpression.GetConditionExpression();
                retVal.PredicateSerialized = expressionSerializer.SerializeExpression(conditionExpression);

                //Clear availableElements in expression (for decrease size)
                publication.DynamicExpression.AvailableChildren = null;
                var allBlocks = ((DynamicExpression)publication.DynamicExpression).Traverse(x => x.Children);
                foreach (var block in allBlocks)
                {
                    block.AvailableChildren = null;
                }
                retVal.PredicateVisualTreeSerialized = JsonConvert.SerializeObject(publication.DynamicExpression);
            }

            return retVal;
        }
    }
}
